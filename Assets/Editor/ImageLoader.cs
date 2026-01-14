#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System;
using System.Collections;
using System.IO;

public static class ImageLoader
{
    public static void LoadSprite(string url, Action<Sprite> onLoaded)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Load(url, onLoaded));
    }

    static IEnumerator Load(string url, Action<Sprite> onLoaded)
    {
        using var req = UnityWebRequest.Get(url);
        req.timeout = 15;
        req.SetRequestHeader("User-Agent", "Mozilla/5.0");
        req.SetRequestHeader("Accept", "*/*");
        req.SetRequestHeader("Accept-Encoding", "identity");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[ImageLoader] FAILED");
            Debug.LogError(req.error);
            onLoaded?.Invoke(null);
            yield break;
        }

        string contentType = req.GetResponseHeader("Content-Type");
        byte[] data = req.downloadHandler.data;

        if (string.IsNullOrEmpty(contentType))
        {
            Debug.LogError("[ImageLoader] No Content-Type");
            onLoaded?.Invoke(null);
            yield break;
        }

        // =========================
        // WebP
        // =========================
        if (contentType.Contains("image/webp"))
        {
            Texture2D tex = WebPDecoder.Decode(data);
            if (tex == null)
            {
                Debug.LogError("[ImageLoader] WebP Decode Failed");
                onLoaded?.Invoke(null);
                yield break;
            }

            onLoaded?.Invoke(CreateSprite(tex));
            yield break;
        }

        // =========================
        // PNG / JPG
        // =========================
        var tex2 = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex2.LoadImage(data))
        {
            Debug.LogError("[ImageLoader] LoadImage failed");
            onLoaded?.Invoke(null);
            yield break;
        }

        onLoaded?.Invoke(CreateSprite(tex2));
    }

    static Sprite CreateSprite(Texture2D tex)
    {
        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
}
#endif
