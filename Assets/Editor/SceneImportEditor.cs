using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class SceneImportEditor
{
    [MenuItem("UniForge/Import Scene From JSON")]
    static void ImportScene()
    {
        EnsureImageLoader();
        EnsureSceneImportManager();

        SceneImportManager manager = Object.FindObjectOfType<SceneImportManager>();
        manager.SceneFromJson();

        Debug.Log("[SceneImportEditor] Import Finished");
    }

    static void EnsureImageLoader()
    {
        var loader = Object.FindObjectOfType<ImageLoader>();
        if (loader != null)
        {
            Debug.Log("[SceneImportEditor] ImageLoader already exists");
            return;
        }

        Debug.Log("[SceneImportEditor] Creating ImageLoader (Editor)");

        GameObject go = new GameObject("@ImageLoader(Editor)");
        go.AddComponent<ImageLoader>();

        // Editor에서는 DontDestroyOnLoad 의미 없음 → 제거
    }

    static void EnsureSceneImportManager()
    {
        if (Object.FindObjectOfType<SceneImportManager>() != null)
            return;

        Debug.Log("[SceneImportEditor] Creating SceneImportManager (Editor)");

        GameObject go = new GameObject("@SceneImportManager(Editor)");
        go.AddComponent<SceneImportManager>();
    }
}
