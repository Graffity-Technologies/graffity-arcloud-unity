using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.Graffity.ARCloud
{
    [CreateAssetMenu(fileName = "ApiCredConfig", menuName = "AR Cloud/Api Credentials Config", order = 1)]
    public class ApiCredConfig : ScriptableObject
    {
        public string graffApiKey;
    }
}

