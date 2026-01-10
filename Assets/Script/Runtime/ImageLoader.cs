using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ImageLoader : MonoBehaviour
{
    public static ImageLoader Instance { get; private set; }

    private Dictionary<string, Sprite> spriteByUrl = new();
    private Dictionary<string, bool> loadingUrls = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ?? 외부에서 호출용
    public void LoadSprite(string url, System.Action<Sprite> onLoaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[ImageLoader] URL is null or empty");
            onLoaded?.Invoke(null);
            return;
        }

        // 이미 캐시됨
        if (spriteByUrl.TryGetValue(url, out var cached))
        {
            onLoaded?.Invoke(cached);
            return;
        }

        // 이미 로딩 중
        if (loadingUrls.ContainsKey(url))
        {
            StartCoroutine(WaitForLoad(url, onLoaded));
            return;
        }

        StartCoroutine(LoadSpriteCoroutine(url, onLoaded));
    }

    IEnumerator LoadSpriteCoroutine(string url, System.Action<Sprite> onLoaded)
    {
        loadingUrls[url] = true;

        using var req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        loadingUrls.Remove(url);

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ImageLoader] Failed to load image: {url}\n{req.error}");
            Debug.LogError($"[ImageLoader] Request Error: {req.error}");
            Debug.LogError($"[ImageLoader] Response Code: {req.responseCode}");
            Debug.LogError($"[ImageLoader] Response Text: {req.downloadHandler.text}");
            onLoaded?.Invoke(null);
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(req);
        var sprite = CreateSprite(tex);

        spriteByUrl[url] = sprite;
        onLoaded?.Invoke(sprite);
    }

    IEnumerator WaitForLoad(string url, System.Action<Sprite> onLoaded)
    {
        yield return new WaitUntil(() => spriteByUrl.ContainsKey(url));
        onLoaded?.Invoke(spriteByUrl[url]);
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
