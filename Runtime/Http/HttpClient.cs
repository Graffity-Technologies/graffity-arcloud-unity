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

    internal async Task<TResultType> Post<TResultType>(string url, string jsonBody)
    {
        try
        {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(rawBody);
            www.downloadHandler = new DownloadHandlerBuffer();

            // using var www = UnityWebRequest.Post(url, data);

            www.SetRequestHeader("Content-Type", _serializationOption.ContentType);
            // TODO: rm hard code api key below
            www.SetRequestHeader("x-graff-console", "YTA4Yjc4NWUtMjgxYi00ZTRmLWFlNjAtNTQ5NmJjZmVlNjdmLTUxNWUxMDE3LWYzNDktNDdlMi05MjBhLTMzODBlMTNhNGQ5YQ==");

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success) // Network Error
                Debug.LogError($"Failed: {www.error}");

            var result = _serializationOption.Deserialize<TResultType>(www.downloadHandler.text);

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"{nameof(Post)} failed: {ex.Message}");
            return default;
        }
    }
}