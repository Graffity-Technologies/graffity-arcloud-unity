namespace UnityEngine.Graffity.ARCloud
{
    [CreateAssetMenu(fileName = "ApiCredConfigDev", menuName = "AR Cloud/Api Credentials Config Dev", order = 2)]
    public class ApiCredConfigDev : ApiCredConfig
    {
        public bool isSecure;
        public bool useCustomSolverServer;
        public string customSolverHost;

        private void OnValidate()
        {
            _isSecure = isSecure;
            _useCustomSolverServer = useCustomSolverServer;
            _customSolverHost = customSolverHost;
        }
    }
}