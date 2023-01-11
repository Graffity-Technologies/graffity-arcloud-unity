using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.Graffity.ARCloud
{
    [CreateAssetMenu(fileName = "ApiCredConfig", menuName = "AR Cloud/Api Credentials Config", order = 1)]
    public class ApiCredConfig : ScriptableObject
    {
        public string consoleAccessToken;
        
        internal bool _isSecure = true;
        internal bool _useCustomSolverServer = false;
        internal string _customSolverHost = "";

        [ContextMenu("Validate Access Token")]
        async void TestAccessToken()
        {
            await AccessTokenController.IsValidAccessToken(consoleAccessToken);
        }


    }
}

