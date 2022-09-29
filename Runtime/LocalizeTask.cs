using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Image;

namespace UnityEngine.Graffity.ARCloud
{
    internal class LocalizeTask
    {
        private const float UPDATE_DISTANCE_THRESH = 0.25f;
        private const int MAX_PENDING_REQUEST = 4;
        
        internal Dictionary<long, PosePair> localizeData;
        private long _lastKey;
        private int pendingRequestCount;
        internal int requirePoint;
        internal LocalizeTaskState state;
        private Vector3 lastArPosition;

        public float progress => (float)localizeData.Count / requirePoint;

        public LocalizeTask(LocalizeStrategy strategy = LocalizeStrategy.LAST_POINT_DIFF_HIGHPRECISION)
        {
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
                    requirePoint = 50;
                    break;
                default:
                    requirePoint = 10;
                    break;
            }
            this.requirePoint = requirePoint;
            state = LocalizeTaskState.CollectingPoint;
            lastArPosition = Vector3.negativeInfinity;
            
        }
        
        internal bool ShouldAddPoint()
        {
            // if (_lastKey == -1)
            //     return true;

            if (pendingRequestCount >= MAX_PENDING_REQUEST)
                return false;
            
            if (lastArPosition == Vector3.negativeInfinity)
                return true;
            
            var arLocalPosition = ARCloudSession.instance.cameraManager.transform.position;
            
            var localDist = Vector3.Distance(arLocalPosition, lastArPosition);
            if (localDist >= UPDATE_DISTANCE_THRESH)
                return true;
            
            return false;
        }

        internal async void AddPoint(Pose arPose, Task<ImageResponse> vpsReqTask)
        {
            var key = HiResDateTime.UtcNowTicks;
            lastArPosition = arPose.Position;
            
            // TODO
            ImageResponse response;
            try
            {
                pendingRequestCount += 1;
                response = await vpsReqTask;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
            finally
            {
                pendingRequestCount -= 1;
            }
            
            if (response == null)
                return;
            if (!response.IsInitialized())
                return;
            if (response.Accuracy == 0f)
                return;
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
                    (float)response.ColmapCoor.Qw)
            };
            
            localizeData.Add(key, new PosePair()
            {
                AR = arPose,
                VPS = vpsPose
            });
            
            _lastKey = key;

            if (localizeData.Count >= requirePoint)
            {
                state = LocalizeTaskState.CollectingPointFinish;
                
                var keys = localizeData.Keys.ToList();
                keys.Sort();
                
                var solveTf = await ARCloudSession.instance.RequestSolveTransformationAsync(
                    keys.Select(k => localizeData[k].AR).ToList(), 
                    keys.Select(k => localizeData[k].VPS).ToList()
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
        LAST_POINT_DIFF_HIGHPRECISION
    }

    internal enum LocalizeTaskState
    {
        CollectingPoint,
        CollectingPointFinish,
        Expire
    }
}