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

        // Localization Task Field
        // public float LocalizeProgress;
        private LocalizeTask currentLocalizeTask;

        public String localizeState => currentLocalizeTask != null ? currentLocalizeTask.state.ToString() : "Deleted";
        public float localizeProgress => currentLocalizeTask?.progress ?? 0f;
        public string localizeProgressMessage => currentLocalizeTask?.progressMessage ?? "N/A";
        private int frameDrop = 15;
        private int captureFrameCounter = 0;

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

            #if UNITY_IPHONE
            frameDrop = 30;
            #endif
        }

        private void Update()
        {
            captureFrameCounter += 1;
            LocalizeTaskUpdate();
        }

        private async void LocalizeTaskUpdate()
        {
            if (currentLocalizeTask == null)
            {
                return;
            }

            switch (currentLocalizeTask.state)
            {
                case LocalizeTaskState.CollectingPoint:
                    // if (currentLocalizeTask.ShouldAddPoint() == false)
                    if (captureFrameCounter % frameDrop != 0)
                    {
                        return;
                    }
                    var cameraTf = cameraManager.transform;
                    var arPose = new Pose()
                    {
                        Position = cameraTf.localPosition,
                        Rotation = cameraTf.localRotation,
                        Translation = cameraTf.localPosition,
                        Covariance = new Google.Protobuf.Collections.RepeatedField<double>() { 1.0, 1.0 },
                        Timestamp = ARCloudUtils.GetMicroseconds().ToString(),
                        Accuracy = 0.0f
                    };
                    // Debug.Log("arPose Covariance: " + arPose.Covariance.ToString()); // ...

                    if (Status != ARCloudSessionStatus.Initialized)
                        // throw new ARCloudException("ARCloudSession not initialized");
                        return;

                    if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                    {
                        image.Dispose();
                        // throw new ARCloudException("Cannot get image");
                        return;
                    }

                    // normally XRCpuImage ratio is 4:3 so scale down to 640:480
                    var imageSizeTarget = 480;
                    var downSizeImageFactor = image.height / imageSizeTarget;
                    if (image.width < image.height)
                    {
                        downSizeImageFactor = image.width / imageSizeTarget;
                    }
                    if (downSizeImageFactor < 1) // Downsample only so shouldn't more than 1 https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/cpu-camera-image.html
                    {
                        downSizeImageFactor = 1;
                    }
                    // Debug.Log($"downSizeImageFactor: {downSizeImageFactor}");
                    var byteImage = await XrImageToPngByteString(image, downSizeImageFactor);
                    image.Dispose();

                    if (byteImage is null)
                        return;

#if UNITY_EDITOR
                            var cameraInfo = new CameraInfo()
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

                    var cameraInfo = new CameraInfo()
                    {
                        PixelFocalLength = ((cameraIntrinsics.focalLength.x + cameraIntrinsics.focalLength.y) / 2) / downSizeImageFactor,
                        PrincipalPointX = cameraIntrinsics.principalPoint.x / downSizeImageFactor,
                        PrincipalPointY = cameraIntrinsics.principalPoint.y / downSizeImageFactor,
                        RadialDistortion = 0
                    };
#endif
                    // Debug.Log(cameraInfo);

                    var sendImageTask = SendImageAsync(byteImage, cameraInfo);
                    currentLocalizeTask.AddPoint(arPose, sendImageTask); // await 
                    // sendImageTask.Dispose();

                    // Debug.Log($"arPose timestamp: {arPose.Timestamp}");

                    break;
                case LocalizeTaskState.Expire:
                    currentLocalizeTask = null;
                    break;
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

        public async Task<ImageResponse> SendImageAsync(ByteString byteImage, CameraInfo cameraData)
        {

            // ImageResponse imageResponse = null;
            // NativeGallery.SaveImageToGallery(byteImage.ToByteArray(), "GraffityTest", $"img{arSessionOrigin.transform.position}.png",
            //     (success, path) => Debug.Log("Media save result: " + success + " " + path));
            try
            {
                return await grpcManager.imageClient.SendGrpcAsync(new ImageRequest
                {
                    Message = "Send request from Unity",
                    BytesImage = byteImage,
                    GpsPosition = (Position)refModelPositionGps,
                    CameraInfo = cameraData,
                    Platform = "UNITY",
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
            request.Platform = "UNITY";
            request.VpsCoordinate.AddRange(vpsPoses.Select(p => p.ToCoordinate()));
            request.ArCoordinate.AddRange(arPoses.Select(p => p.ToCoordinate()));
            // Debug.Log("ArCoordinate ToCoordinate Result: " + request.ArCoordinate.ToString());

            if (!string.IsNullOrEmpty(message))
                request.Message = message;
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