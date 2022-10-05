using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Image;
using Solver;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.Graffity.ARCloud.ARCloudUtils;

namespace UnityEngine.Graffity.ARCloud
{
    public class ARCloudSession : MonoBehaviour
    {
        public static ARCloudSession instance { get; protected set; }
        
        [SerializeField] 
        private ApiCredConfig apiCredConfig;
        
        [SerializeField] 
        private Transform refPointCloudTf;

        public ARCloudSessionStatus Status { get; private set; }
        private VpsGrpcManager grpcManager;
        internal ARCameraManager cameraManager;
        private ARSessionOrigin arSessionOrigin;
        
        private PositionGps refModelPositionGps;
        private CameraInfo cameraInfo;
        
        // Localization Task Field
        // public float LocalizeProgress;
        private LocalizeTask currentLocalizeTask;
        
        public String localizeState => currentLocalizeTask != null ? currentLocalizeTask.state.ToString() : "Deleted";
        public float localizeProgress => currentLocalizeTask?.progress ?? 0f;


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                throw new Exception("An instance of ARCloudSession already exists.");
            }
            else
            {
                instance = (ARCloudSession)this;
            }
        }
        private void Start()
        {
            grpcManager = new VpsGrpcManager(apiCredConfig);
            cameraManager = gameObject.GetComponentInChildren<ARCameraManager>();
            arSessionOrigin = gameObject.GetComponent<ARSessionOrigin>();
            Status = ARCloudSessionStatus.Uninitialized;

        }

        private void Update()
        {
#if UNITY_EDITOR
            //TODO add editor t
#else
            LocalizeTaskUpdate();
#endif
        }

        private void LocalizeTaskUpdate()
        {
            if (currentLocalizeTask != null)
            {
                switch (currentLocalizeTask.state)
                {
                    case LocalizeTaskState.CollectingPoint:
                        if (currentLocalizeTask.ShouldAddPoint())
                        {
                            var cameraTf = cameraManager.transform;
                            var arPose = new Pose()
                            {
                                Position = cameraTf.localPosition,
                                Rotation = cameraTf.localRotation
                            };
                            currentLocalizeTask.AddPoint(arPose, SendImageAsync());
                        }
                        break;
                    case LocalizeTaskState.Expire:
                        currentLocalizeTask = null;
                        break;
                }
            }
        }

        public async Task Init(PositionGps referencePositionGps)
        {
#if UNITY_EDITOR
            cameraInfo = new CameraInfo()
            {
                PixelFocalLength = 200,
                PrincipalPointX = 320,
                PrincipalPointY = 240,
                RadialDistortion = 0
            };
#else
            if (!cameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
            {
                throw new ARCloudExceptionNotAvailable("Cannot get cameraIntrinsics");
            }
            
            cameraInfo = new CameraInfo()
            {
                PixelFocalLength = (cameraIntrinsics.focalLength.x + cameraIntrinsics.focalLength.y) / 2,
                PrincipalPointX = cameraIntrinsics.principalPoint.x,
                PrincipalPointY = cameraIntrinsics.principalPoint.y,
                RadialDistortion = 0
            };
#endif

            refModelPositionGps = referencePositionGps;

            grpcManager.ConnectServers();
            
            if (!await grpcManager.ValidateConnections())
            {
                throw new ARCloudExceptionNotAvailable("Server connection is not valid");
            }
            
            Status = ARCloudSessionStatus.Initialized;
        }

        public void StartLocalize(LocalizeStrategy strategy = LocalizeStrategy.LAST_POINT_DIFF_MEDPRECISION)
        {
            if (Status != ARCloudSessionStatus.Initialized)
                throw new ARCloudException("ARCloudSession not initialized");

            if (currentLocalizeTask != null)
                currentLocalizeTask.state = LocalizeTaskState.Expire;
                
            currentLocalizeTask = new LocalizeTask(strategy);
        }

        internal void AdjustOriginPose(Vector3 translation, Vector3 Scale, Quaternion Rotation)
        {
            var arOriginTf = arSessionOrigin.transform;
            arOriginTf.position = translation;
            arOriginTf.localScale = Scale;
            arOriginTf.rotation = Rotation;
        }

        public async Task<ImageResponse> SendImageAsync()
        {
            if (Status != ARCloudSessionStatus.Initialized)
                throw new ARCloudException("ARCloudSession not initialized");

            if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                Debug.Log("Cannot get image");
                throw new ARCloudException("Cannot get image");
            }

            // TODO remove logging
            XrImageToPngBytes(ref image, out byte[] buffer);
            image.Dispose();
            ImageResponse imageResponse = null; 
            await grpcManager.SendImageAsync(new ImageRequest
                {
                    BytesImage = ByteString.CopyFrom(buffer),
                    GpsPosition = (Position)refModelPositionGps,
                    CameraInfo = cameraInfo
                
                }, (response) =>
                {
                    imageResponse = response;
                    // NativeGallery.SaveImageToGallery(buffer, "GraffityTest", $"test.png",
                    //     (success, path) => Debug.Log("Media save result: " + success + " " + path));
                },
                (e) => Debug.Log(e.Message));
            return imageResponse;
        }

        public async Task<bool> MockStartLocalizeAsync(List<Pose> arPoses, List<Pose> vpsPoses)
        {
            var solveTf = await RequestSolveTransformationAsync(arPoses, vpsPoses);
            AdjustOriginPose(solveTf.Translation, solveTf.Scale, solveTf.Rotation);
            return true;
        }
        
        internal async Task<SolveTransformation> RequestSolveTransformationAsync(ICollection<Pose> arPoses, ICollection<Pose> vpsPoses)
        {
            if (Status != ARCloudSessionStatus.Initialized)
                throw new ARCloudException("ARCloudSession not initialized");
            
            var request = new SolveRequest()
            {
                RefTransform = new Transformation()
                {
                    Translation = refPointCloudTf.position.ToVec3(),
                    Scale = refPointCloudTf.localScale.ToVec3(),
                    Rotation = refPointCloudTf.rotation.ToVec4()
                }
            };
            request.ArCoordinate.AddRange(arPoses.Select(p => p.ToCoordinate()));
            request.VpsCoordinate.AddRange(vpsPoses.Select(p => p.ToCoordinate()));

            var response = await grpcManager.RequestSolveAsync(request);
            var solveTransformation = new SolveTransformation()
            {
                Translation = response.Transform.Translation.ToVector3(),
                Scale = response.Transform.Scale.ToVector3(),
                Rotation = response.Transform.Rotation.ToVec4()
            };
            return solveTransformation;
            
        }
        
    }

    public enum ARCloudSessionStatus
    {
        Uninitialized,
        Initialized,
        Expired
    }
    
    internal enum ARCloudLocalizeStatus
    {
        NotActive,
        Active
    }
}