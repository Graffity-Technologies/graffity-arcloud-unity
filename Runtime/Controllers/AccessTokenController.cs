using System.Threading.Tasks;

namespace UnityEngine.Graffity.ARCloud
{
    public static class AccessTokenController
    {
        const string GraffityConsoleBackendURL = "https://console-backend.graffity.tech"; // "http://localhost:8080";

        public static async Task<bool> IsValidAccessToken(string rawToken)
        {
            var url = GraffityConsoleBackendURL + "/api/v2/access-token/validate";

            var httpClient = new HttpClient(new JsonSerializationOption());
            var token = new AccessTokenModel();
            token.Raw = rawToken;

            string json = JsonUtility.ToJson(token);

            var result = await httpClient.Post<string>(url, json, rawToken);

            if (result == "")
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

        public async static void TestAccessToken()
        {
            var a = await IsValidAccessToken("");
            Debug.Log(a);
        }
    }
}

