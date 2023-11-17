using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Vpsimage;

namespace UnityEngine.Graffity.ARCloud
{
    internal class LocalizeTask
    {
        private const float UPDATE_DISTANCE_THRESH = 0.5f;
        private const float UPDATE_ROT_THRESH = Mathf.PI / 5;
        private const int MAX_PENDING_REQUEST = 4;

        private LocalizeStrategy strategy;

        internal Dictionary<long, PosePair> localizeData;
        private long _lastKey;
        private int pendingRequestCount;
        internal int requirePoint;
        internal LocalizeTaskState state;
        private Vector3 lastArPosition;
        private Quaternion lastArRotation;
        private string solverGuideMessage = null;

        private int missPoint;

        public float progress => (float)localizeData.Count / requirePoint;
        public string progressMessage => $"{localizeData.Count} / {requirePoint} : {pendingRequestCount} x {missPoint} || {strategy.ToString()}";

        public LocalizeTask(LocalizeStrategy strategy)
        {
            this.strategy = strategy;
            localizeData = new Dictionary<long, PosePair>();
            _lastKey = -1;
            pendingRequestCount = 0;
            switch (strategy)
            {
                case LocalizeStrategy.LAST_POINT_DIFF:
                    requirePoint = 10;
                    break;
                case LocalizeStrategy.LAST_POINT_DIFF_MEDPRECISION:
                    requirePoint = 20;
                    break;
                case LocalizeStrategy.LAST_POINT_DIFF_HIGHPRECISION:
                    requirePoint = 40;
                    break;
                case LocalizeStrategy.ANGLE_SOLVER:
                    requirePoint = 10;
                    solverGuideMessage = "ANGLE_SOLVER";
                    break;
                default:
                    requirePoint = 10;
                    break;
            }
            this.requirePoint = requirePoint;
            state = LocalizeTaskState.CollectingPoint;
            lastArPosition = Vector3.negativeInfinity;
            lastArRotation = Quaternion.identity;

        }

        internal bool ShouldAddPoint()
        {
            // if (_lastKey == -1)
            //     return true;

            if (pendingRequestCount >= MAX_PENDING_REQUEST)
                return false;

            if (lastArPosition == Vector3.negativeInfinity)
                return true;

            // global position ensure user canvas scale
            var arLocalPosition = ARCloudSession.instance.cameraManager.transform.localPosition;
            var arLocalRotation = ARCloudSession.instance.cameraManager.transform.localRotation;

            var localDist = Vector3.Distance(arLocalPosition, lastArPosition);
            if (localDist >= UPDATE_DISTANCE_THRESH)
                return true;

            if (strategy == LocalizeStrategy.ANGLE_SOLVER)
            {
                var localRotDist = Quaternion.Dot(lastArRotation, arLocalRotation);
                if (localRotDist >= UPDATE_ROT_THRESH)
                    return true;
            }

            return false;
        }



        internal async Task AddPoint(Pose arPose, Task<ImageResponse> vpsReqTask)
        {
            var key = HiResDateTime.UtcNowTicks;
            lastArPosition = arPose.Position;
            lastArRotation = arPose.Rotation;

            // TODO
            ImageResponse response;
            try
            {
                pendingRequestCount += 1;
                response = await vpsReqTask;
            }
            catch (Exception e)
            {
                // Debug.Log(e);
                missPoint += 1;
                Debug.LogWarning($"miss point exception {e}");
                return;
            }
            finally
            {
                pendingRequestCount -= 1;
            }

            if (response == null)
            {
                Debug.LogWarning("miss point response null");
                missPoint += 1;
                return;
            }

            if (!response.IsInitialized())
            {
                Debug.LogWarning("miss point response not initialized");
                missPoint += 1;
                return;
            }
            // if (response.Accuracy == 0f)
            // {
            //     Debug.LogWarning("miss point response accuracy 0");
            //     missPoint += 1;
            //     return;
            // }

            var vpsPose = new Pose()
            {
                Position = new Vector3(
                    (float)response.ColmapCoor.Px,
                    (float)response.ColmapCoor.Py,
                    (float)response.ColmapCoor.Pz),
                Rotation = new Quaternion(
                    (float)response.ColmapCoor.Qx,
                    (float)response.ColmapCoor.Qy,
                    (float)response.ColmapCoor.Qz,
                    (float)response.ColmapCoor.Qw),
                Translation = new Vector3(
                    (float)response.ColmapCoor.Tx,
                    (float)response.ColmapCoor.Ty,
                    (float)response.ColmapCoor.Tz),
                Timestamp = ARCloudUtils.GetMicroseconds().ToString(),
                Accuracy = (float)response.Accuracy,
                Covariance = response.Covariance,
            };
            // Debug.Log("Accuracy: " + vpsPose.Accuracy.ToString());
            // Debug.Log("vpsPose.Covariance: " + vpsPose.Covariance.ToString());
            // Debug.Log("response.Covariance: " + response.Covariance.ToString());

            localizeData.Add(key, new PosePair()
            {
                AR = arPose,
                VPS = vpsPose
            });

            _lastKey = key;
            if (localizeData.Count == requirePoint)
            {
                state = LocalizeTaskState.CollectingPointFinish;

                var keys = localizeData.Keys.ToList();
                keys.Sort();

                var solveTf = await ARCloudSession.instance.RequestSolveTransformationAsync(
                    keys.Select(k => localizeData[k].AR).ToList(),
                    keys.Select(k => localizeData[k].VPS).ToList(),
                    solverGuideMessage
                );

                if (state != LocalizeTaskState.Expire)
                {
                    ARCloudSession.instance.AdjustOriginPose(solveTf.Translation, solveTf.Scale, solveTf.Rotation);
                }

                state = LocalizeTaskState.Expire;
            }

        }
    }

    public enum LocalizeStrategy
    {
        LAST_POINT_DIFF,
        LAST_POINT_DIFF_MEDPRECISION,
        LAST_POINT_DIFF_HIGHPRECISION,
        ANGLE_SOLVER
    }

    internal enum LocalizeTaskState
    {
        CollectingPoint,
        CollectingPointFinish,
        Expire
    }
}