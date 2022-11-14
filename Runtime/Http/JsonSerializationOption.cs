using System;
using UnityEngine;

internal class JsonSerializationOption : ISerializationOption
{
    public string ContentType => "application/json";

    public T Deserialize<T>(string jsonString)
    {
        try
        {
            var result = JsonUtility.FromJson<T>(jsonString);
            // Debug.Log($"Success: {jsonString}");
            return result;
        }
        catch (Exception ex)
        {
            // Debug.LogError($"Could not parse response {jsonString}. {ex.Message}");
            return default;
        }
    }
}