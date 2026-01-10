using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RuntimeAssetManager : MonoBehaviour
{
    public static RuntimeAssetManager Instance;

    // id, url 전부 string 기준
    private Dictionary<string, Sprite> spriteById = new();
    private Dictionary<string, Sprite> spriteByUrl = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // AssetDTO 기반 로딩
    public IEnumerator LoadAsset(AssetDTO asset)
    {
        if (string.IsNullOrEmpty(asset.id))
        {
            Debug.LogWarning("[RuntimeAssetManager] Asset id is null or empty");
            yield break;
        }

        if (spriteById.ContainsKey(asset.id))
            yield break;

        yield return LoadSpriteFromUrl(asset.url, sprite =>
        {
            spriteById[asset.id] = sprite;

            if (!string.IsNullOrEmpty(asset.url))
                spriteByUrl[asset.url] = sprite;
        });
    }

    //id 기반 접근 (UUID)
    public Sprite GetSprite(string assetId)
        => spriteById.TryGetValue(assetId, out var s) ? s : null;

    // url 기반 접근
    public Sprite GetSpriteByUrl(string url)
        => spriteByUrl.TryGetValue(url, out var s) ? s : null;

    IEnumerator LoadSpriteFromUrl(string url, System.Action<Sprite> onLoaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("[RuntimeAssetManager] Asset url is null or empty");
            yield break;
        }

        UnityWebRequest uwr;

        //로컬 파일
        if (!url.StartsWith("http"))
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, url);
            uwr = UnityWebRequestTexture.GetTexture(path);
        }
        // 원격 이미지
        else
        {
            uwr = UnityWebRequestTexture.GetTexture(url);
        }

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[RuntimeAssetManager] Failed to load image: {url}\n{uwr.error}");
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(uwr);
        if (tex == null)
        {
            Debug.LogError($"[RuntimeAssetManager] Texture decode failed: {url}");
            yield break;
        }

        onLoaded(CreateSprite(tex));
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
