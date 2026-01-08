using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class WebAssetImportHandler
{
    public static void Handle(string json)
    {
        var data = JsonUtility.FromJson<WebAssetPayload>(json);

        byte[] bytes = Convert.FromBase64String(data.base64);
        string path = Path.Combine(
            Application.streamingAssetsPath,
            data.path
        );

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, bytes);

        AssetDatabase.Refresh();
        Debug.Log($" Asset Imported: {path}");
    }
}

[Serializable]
public class WebAssetPayload
{
    public string path;
    public string base64;
}
