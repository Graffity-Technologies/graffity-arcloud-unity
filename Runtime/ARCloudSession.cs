using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Vpsimage;
using Solver;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.Graffity.ARCloud.ARCloudUtils;

namespace UnityEngine.Graffity.ARCloud
{
    public class ARCloudSession : MonoBehaviour
    {
        public static ARCloudSession instance { get; protected set; }

        public ApiCredConfig apiCredConfig;

        [SerializeField]
        private Transform refPointCloudTf;

        public ARCloudSessionStatus Status { get; private set; }
        public VpsGrpcManager grpcManager;
        internal ARCameraManager cameraManager;
        private ARSessionOrigin arSessionOrigin;

        private PositionGps refModelPositionGps;
        private CameraInfo cameraInfo;

        // Localization Task Field
        // public float LocalizeProgress;
        private LocalizeTask currentLocalizeTask;

        public String localizeState => currentLocalizeTask != null ? currentLocalizeTask.state.ToString() : "Deleted";
        public float localizeProgress => currentLocalizeTask?.progress ?? 0f;
        public string localizeProgressMessage => currentLocalizeTask?.progressMessage ?? "N/A";

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
            LocalizeTaskUpdate();
#else
            LocalizeTaskUpdate();
#endif
        }

        private async void LocalizeTaskUpdate()
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

                            if (Status != ARCloudSessionStatus.Initialized)
                                // throw new ARCloudException("ARCloudSession not initialized");
                                return;

                            if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                            {
                                image.Dispose();
                                // throw new ARCloudException("Cannot get image");
                                return;
                            }

                            var byteImage = await XrImageToPngByteString(image);
                            image.Dispose();

                            if (byteImage is null)
                                return;

                            var sendImageTask = SendImageAsync(byteImage);
                            await currentLocalizeTask.AddPoint(arPose, sendImageTask);
                            sendImageTask.Dispose();

                            Debug.Log("");
                        }
                        break;
                    case LocalizeTaskState.Expire:
                        currentLocalizeTask = null;
                        break;
                }
            }
        }

        public bool CheckIsAreaAvailable(PositionGps positionGps)
        {
            var request = new AvailableAreaRequest
            {
                GpsPosition = (Position)positionGps,
                MaxDistance = 1000,
                MinDistance = 0
            };

            return grpcManager.CheckAvailableArea(request).IsAvailable;
        }

        public async Task<bool> CheckIsAreaAvailableAsync(PositionGps positionGps)
        {
            var request = new AvailableAreaRequest
            {
                GpsPosition = (Position)positionGps,
                MaxDistance = 1000,
                MinDistance = 0
            };

            var response = await grpcManager.CheckAvailableAreaAsync(request);
            return response.IsAvailable;

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
            Debug.Log(cameraInfo);
            refModelPositionGps = referencePositionGps;

            var accessToken = apiCredConfig.consoleAccessToken;
            if (!await AccessTokenController.IsValidAccessToken(accessToken))
            {
                throw new ARCloudExceptionNotAvailable("Access token not valid");
            }

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

        public async Task<ImageResponse> MockSendImageAsync()
        {
            await Task.Delay(5);
            return new ImageResponse
            {
                Message = "",
                Accuracy = 0,
                ColmapCoor = new ColmapCoordinate
                {
                    Qw = 0,
                    Qx = 0,
                    Qy = 0,
                    Qz = 0,
                    Tx = 0,
                    Ty = 0,
                    Tz = 0,
                    Px = 0,
                    Py = 0,
                    Pz = 0
                },
                WorldCoor = new WorldCoordinate
                {
                    Latitude = 0,
                    Longitude = 0,
                    Altitude = 0,
                    UtmX = 0,
                    UtmY = 0
                },

            };
        }

        public async Task<ImageResponse> SendImageAsync(ByteString byteImage)
        {

            // ImageResponse imageResponse = null;
            // NativeGallery.SaveImageToGallery(byteImage.ToByteArray(), "GraffityTest", $"img{arSessionOrigin.transform.position}.png",
            //     (success, path) => Debug.Log("Media save result: " + success + " " + path));
            try
            {
                return await grpcManager.imageClient.SendGrpcAsync(new ImageRequest
                {
                    BytesImage = byteImage,
                    GpsPosition = (Position)refModelPositionGps,
                    CameraInfo = cameraInfo
                }) as ImageResponse;
            }
            catch (Exception e)
            {
                Debug.LogError($"Send image raise Exception: {e.Message} {e.StackTrace}");
                return null;
            }


            // , (response) =>
            // {
            //     imageResponse = response;
            //     // NativeGallery.SaveImageToGallery(buffer, "GraffityTest", $"img{arSessionOrigin.transform.position}.png",
            //     //     (success, path) => Debug.Log("Media save result: " + success + " " + path));
            // },
            // (e) =>
            // {
            //     // NativeGallery.SaveImageToGallery(buffer, "GraffityTest", $"img-FAIL-{arSessionOrigin.transform.position}.png",
            //     //     (success, path) => Debug.Log("Media save result: " + success + " " + path));
            //     Debug.Log($"Send image fail {e.Message}");
            // });
        }

        public async Task<bool> MockStartLocalizeAsync(List<Pose> arPoses, List<Pose> vpsPoses)
        {
            var solveTf = await RequestSolveTransformationAsync(arPoses, vpsPoses);
            AdjustOriginPose(solveTf.Translation, solveTf.Scale, solveTf.Rotation);
            return true;
        }

        internal async Task<SolveTransformation> RequestSolveTransformationAsync(ICollection<Pose> arPoses, ICollection<Pose> vpsPoses, string message = null)
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
            if (string.IsNullOrEmpty(message))
                request.Message = "";

            var response = await grpcManager.RequestSolveAsync(request);
            var solveTransformation = new SolveTransformation()
            {
                Translation = response.Transform.Translation.ToVector3(),
                Scale = response.Transform.Scale.ToVector3(),
                Rotation = response.Transform.Rotation.ToVec4()
            };
            return solveTransformation;

        }

        [ContextMenu("Test Validate Access Token")]
        void TestAccessToken()
        {
            AccessTokenController.TestAccessToken();
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