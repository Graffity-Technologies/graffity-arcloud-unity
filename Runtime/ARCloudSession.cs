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
        private LocalizeTask currentLocalizeTask;

        public String localizeState => currentLocalizeTask != null ? currentLocalizeTask.state.ToString() : "Deleted";
        public float localizeProgress => currentLocalizeTask?.progress ?? 0f;
        public bool isInitialized = false;
        private int frameDrop = 15;
        private int captureFrameCounter = 0;
        private int capturedFrames = 0;
        private int filterPitchDegree = 45;
        private int imageSizeTarget = 480;
        public bool localizedFailedOnAllAttempt = false;
        public string arSessionId = Guid.NewGuid().ToString();

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
                    captureFrameCounter += 1;
                    // if (currentLocalizeTask.ShouldAddPoint())
                    if ((captureFrameCounter % frameDrop != 0) ||
                        (capturedFrames >= currentLocalizeTask.vpsSolverBatchSize * (currentLocalizeTask.currentAttemptForSolver + 1)))
                    {
                        return;
                    }
                    // var watch = new System.Diagnostics.Stopwatch();
                    // watch.Start();

                    var arPoseRot = cameraManager.transform.localRotation.eulerAngles;
                    if ((90 - filterPitchDegree > arPoseRot.x & arPoseRot.x > 0) ||
                        (270 + filterPitchDegree < arPoseRot.x & arPoseRot.x < 360))
                    {
                        capturedFrames += 1;
                        // Debug.Log("arPoseRot x: " + arPoseRot.x.ToString());
                        // Debug.Log("capturedFrames: " + capturedFrames.ToString());
                        var cameraTf = cameraManager.transform;
                        var imageId = Guid.NewGuid().ToString();
                        var arPose = new Pose()
                        {
                            Position = cameraTf.localPosition,
                            Rotation = cameraTf.localRotation,
                            Translation = cameraTf.localPosition,
                            Covariance = new Google.Protobuf.Collections.RepeatedField<double>() { 1.0, 1.0 },
                            Timestamp = ARCloudUtils.GetMicroseconds().ToString(),
                            Accuracy = 0.0f,
                            Id = imageId,
                        };
                        // Debug.Log("arPose cameraTf.localRotation x: " + cameraTf.localRotation.eulerAngles.x.ToString());

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
                        var downSizeImageFactor = image.height / imageSizeTarget;
                        if (image.width < image.height)
                        {
                            downSizeImageFactor = image.width / imageSizeTarget;
                        }
                        if (downSizeImageFactor < 1) // Downsample only so shouldn't more than 1
                        {
                            downSizeImageFactor = 1;
                        }

                        // https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/Camera/image-capture.html
                        // 2x faster and 2x smaller than png convertor
                        var byteImage = await XrImageToJpgByteString(image, downSizeImageFactor); // XrImageToPngByteString
                        image.Dispose();

                        if (byteImage is null)
                            return;

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
                        // Debug.Log("downSizeImageFactor: " + downSizeImageFactor.ToString());
                        // Debug.Log("cameraInfo resolution: " + cameraIntrinsics.resolution.ToString());
                        // Debug.Log("cameraInfo focalLength: " + cameraIntrinsics.focalLength.ToString());
                        // Debug.Log("cameraInfo principalPoint: " + cameraIntrinsics.principalPoint.ToString());

                        var sendImageTask = SendImageAsync(byteImage, cameraInfo, arSessionId, imageId);
                        // don't await due to we need smooth sequence of images. so, it not block & ignore warning await with _ =
                        _ = currentLocalizeTask.AddPoint(arPose, sendImageTask);
                        // sendImageTask.Dispose();

                        // watch.Stop();
                        // Debug.Log("Image proces each frame (s):" + watch.ElapsedMilliseconds.ToString());
                    }
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

        public void StartLocalize()
        {
            if (Status != ARCloudSessionStatus.Initialized)
                throw new ARCloudException("ARCloudSession not initialized");

            if (currentLocalizeTask != null)
                currentLocalizeTask.state = LocalizeTaskState.Expire;

            currentLocalizeTask = new LocalizeTask();
        }

        internal void AdjustOriginPose(Vector3 translation, Vector3 Scale, Quaternion Rotation)
        {
            var arOriginTf = arSessionOrigin.transform;
            arOriginTf.position = translation;
            arOriginTf.localScale = Scale;
            // arOriginTf.rotation = Rotation;

            // change y only due to AR foundation is always align to real world
            // so, rotate x,z doen't matter and actually make it worst result because angle diff error
            var resultQuaternion = Quaternion.Euler(0.0f, Rotation.eulerAngles.y, 0.0f);
            arOriginTf.rotation = resultQuaternion;

            isInitialized = true;
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

        public async Task<ImageResponse> SendImageAsync(
            ByteString byteImage, CameraInfo cameraData, string arSessionId, string imageId)
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
                    Id = imageId,
                    Platform = "UNITY",
                    ArSessionId = arSessionId,
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
            var solveResponse = await RequestSolveTransformationAsync(arPoses, vpsPoses);
            var solveTf = new SolveTransformation()
            {
                Translation = solveResponse.Transform.Translation.ToVector3(),
                Scale = solveResponse.Transform.Scale.ToVector3(),
                Rotation = solveResponse.Transform.Rotation.ToVec4()
            };
            AdjustOriginPose(solveTf.Translation, solveTf.Scale, solveTf.Rotation);
            return true;
        }

        internal async Task<SolveResponse> RequestSolveTransformationAsync(ICollection<Pose> arPoses, ICollection<Pose> vpsPoses, string message = null)
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
            request.Id = Guid.NewGuid().ToString();
            request.ArSessionId = arSessionId;
            request.VpsCoordinate.AddRange(vpsPoses.Select(p => p.ToCoordinate()));
            request.ArCoordinate.AddRange(arPoses.Select(p => p.ToCoordinate()));
            // Debug.Log("ArCoordinate ToCoordinate Result: " + request.ArCoordinate.ToString());

            if (!string.IsNullOrEmpty(message))
                request.Message = message;
            var response = await grpcManager.RequestSolveAsync(request);
            return response;
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