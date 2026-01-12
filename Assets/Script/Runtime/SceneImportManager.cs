using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SceneImportManager : MonoBehaviour
{
    [Header("JSON File Name")]
    public string jsonFileName = "scene.json";

    [Header("Prefab Root Path")]
    public string prefabPath = "Prefabs";

    private SceneDTO currentScene;
    private List<AssetDTO> assets;

    [ContextMenu("Load Scene From JSON")]
    public void SceneFromJson()
    {
        LoadSceneFromJson();
        Debug.Log("[SceneImport] Scene loaded");
    }

    void LoadSceneFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"[SceneImport] JSON not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);

        GameDTO game = JsonConvert.DeserializeObject<GameDTO>(json);

        if (game == null)
        {
            Debug.LogError("[SceneImport] GameDTO deserialize failed");
            return;
        }

        currentScene = game.scenes.Find(s => s.sceneId == game.activeSceneId);
        assets = game.assets;

        if (currentScene == null)
        {
            Debug.LogError("[SceneImport] Active scene not found");
            return;
        }

        Debug.Log($"[SceneImport] Load Scene: {currentScene.sceneId}");

        foreach (var entity in currentScene.entities)
        {
            CreateEntity(entity);
        }
    }

    void CreateEntity(EntityDTO entity)
    {
        // 1. GameObject 생성
        GameObject go = new GameObject(entity.name);
        go.transform.position = new Vector3(entity.x, entity.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();

        Debug.Log($"[CreateEntity] Created entity: {entity.name}");

        // 2. Asset 찾기
        AssetDTO asset = FindAssetForEntity(entity);
        if (asset == null)
        {
            Debug.LogWarning($"[CreateEntity] Asset not found for entity: {entity.name}");
            return;
        }

        if (string.IsNullOrEmpty(asset.url))
        {
            Debug.LogWarning($"[CreateEntity] Asset url is empty: {entity.name}");
            return;
        }

        var imageLoader = Object.FindObjectOfType<ImageLoader>();
        if (imageLoader == null)
        {
            Debug.LogError("[CreateEntity] ImageLoader not found in scene");
            return;
        }

        // 4. 이미지 로드
        imageLoader.LoadSprite(asset.url, sprite =>
        {
            if (sprite == null)
            {
                Debug.LogError($"[CreateEntity] Sprite load failed: {asset.url}");
                return;
            }

            sr.sprite = sprite;
            Debug.Log($"[CreateEntity] Sprite applied: {entity.name}");
        });

        // 변수
        if (entity.variables != null && entity.variables.Count > 0)
        {
            CreateVariables(go, entity.variables);
            Debug.Log("변수창출");
        }

        // 이벤트
        if (entity.events != null && entity.events.Count > 0)
        {
            var events = go.AddComponent<RuntimeEvents>();
            events.Initialize(entity.events, go);
        }
    }

    AssetDTO FindAssetForEntity(EntityDTO entity)
    {
        return assets.Find(a =>
            a.name == entity.name
        );
    }

    void CreateVariables(GameObject go, List<VariableDTO> vars)
    {
        var container = go.AddComponent<RuntimeVariables>();

        foreach (var dto in vars)
        {
            VariableSO so = VariableSOFactory.Create(dto);
            if (so != null)
            {
                container.AddVariable(so);
            }
        }
    }
}