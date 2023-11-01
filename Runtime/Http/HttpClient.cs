using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

internal class HttpClient
{
    private readonly ISerializationOption _serializationOption;

    internal HttpClient(ISerializationOption serializationOption)
    {
        _serializationOption = serializationOption;
    }

    internal async Task<TResultType> Post<TResultType>(string url, string jsonBody, string token)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(rawBody);
        www.downloadHandler = new DownloadHandlerBuffer();

        TResultType result;
        try
        {
            // using var www = UnityWebRequest.Post(url, data);

            www.SetRequestHeader("Content-Type", _serializationOption.ContentType);
            // TODO: rm hard code api key below
            www.SetRequestHeader("x-graff-console", token);

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success) // Network Error
                Debug.LogError($"Failed: {www.error}");

            result = _serializationOption.Deserialize<TResultType>(www.downloadHandler.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{nameof(Post)} failed: {ex.Message}");
            result = default;
        }

        www.Dispose();
        return result;
    }

    internal async Task<string> PostStringReturn(string url, string jsonBody, string token)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(rawBody);
        www.downloadHandler = new DownloadHandlerBuffer();

        string result = "";
        try
        {
            // using var www = UnityWebRequest.Post(url, data);

            www.SetRequestHeader("Content-Type", _serializationOption.ContentType);
            // TODO: rm hard code api key below
            www.SetRequestHeader("x-graff-console", token);

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success) // Network Error
                Debug.LogError($"Failed: {www.error}");

            result = "YES"; // _serializationOption.Deserialize<TResultType>(www.downloadHandler.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{nameof(Post)} failed: {ex.Message}");
            result = default;
        }

        www.Dispose();
        return result;
    }
}