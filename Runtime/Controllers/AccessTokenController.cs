using System.Threading.Tasks;

namespace UnityEngine.Graffity.ARCloud
{
    internal class AccessTokenController : MonoBehaviour
    {
        const string GraffityConsoleBackendURL = "https://console-backend-xoeyqjvd6q-as.a.run.app"; // "http://localhost:8080";

        internal async Task<bool> IsValidAccessToken(string rawToken)
        {
            var url = GraffityConsoleBackendURL + "/api/v1/access-token/validate";

            var httpClient = new HttpClient(new JsonSerializationOption());
            var token = new AccessTokenModel();
            token.Raw = rawToken;

            string json = JsonUtility.ToJson(token);

            var result = await httpClient.Post<AccessTokenModel>(url, json);

            if (result == null)
            {
                Debug.LogError("Wrong Access Token!");
                return false;
            }
            else
            {
                Debug.Log("Success to validate the token!");
                return true;
            }
        }

        [ContextMenu("Test Validate Access Token")]
        async void TestAccessToken()
        {
            var a = await IsValidAccessToken("sk.SEMwSGpWZURsbFBocVBTZ3FYWC1LZkFrQlA5ZkdHM09SR1k2ZVZGeHNvWlpQQldNc3Z0T0pUT2JlWTZJUnhxcmNlSERvLUdmcDgzR1hsSzRyUGVFLWJSUkJaVmRzTW1W");
            Debug.Log(a);
        }
    }
}

