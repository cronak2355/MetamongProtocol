using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageLoader : MonoBehaviour
{
    public static ImageLoader Instance { get; private set; }

    private Dictionary<string, Sprite> spriteByUrl = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
            return;
        }

        Instance = this;
    }

    public void LoadSprite(string url, System.Action<Sprite> onLoaded)
    {
#if UNITY_EDITOR
        //  Editor에서는 동기 로딩
        LoadSpriteEditor(url, onLoaded);
#else
        StartCoroutine(LoadSpriteCoroutine(url, onLoaded));
#endif
    }

#if UNITY_EDITOR
    void LoadSpriteEditor(string url, System.Action<Sprite> onLoaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            onLoaded?.Invoke(null);
            return;
        }

        try
        {
            using var req = UnityWebRequestTexture.GetTexture(url);
            var op = req.SendWebRequest();

            while (!op.isDone) { } // Editor에서는 허용

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ImageLoader][Editor] Load failed: {url}\n{req.error}");
                onLoaded?.Invoke(null);
                return;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            var sprite = CreateSprite(tex);

            spriteByUrl[url] = sprite;
            onLoaded?.Invoke(sprite);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            onLoaded?.Invoke(null);
        }
    }
#endif

    IEnumerator LoadSpriteCoroutine(string url, System.Action<Sprite> onLoaded)
    {
        using var req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ImageLoader] Load failed: {url}\n{req.error}");
            onLoaded?.Invoke(null);
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(req);
        var sprite = CreateSprite(tex);

        spriteByUrl[url] = sprite;
        onLoaded?.Invoke(sprite);
    }

    Sprite CreateSprite(Texture2D tex)
    {
        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
}
