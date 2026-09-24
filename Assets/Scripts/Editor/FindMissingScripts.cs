using UnityEditor;
using UnityEngine;

public static class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void Find()
    {
        int totalMissing = 0;

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            totalMissing += CountMissingOnGameObjectAndChildren(prefab, path, true);
        }

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

        // Nhớ lại các scene đang mở để mở lại sau khi quét xong, tránh mất tiến trình đang làm
        var originalSetup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Chỉ mở scene thật sự nằm trong Assets/, bỏ qua bất cứ gì khác
            if (!path.StartsWith("Assets/"))
                continue;

            try
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                    path, UnityEditor.SceneManagement.OpenSceneMode.Additive);

                GameObject[] roots = scene.GetRootGameObjects();
                foreach (GameObject root in roots)
                {
                    totalMissing += CountMissingOnGameObjectAndChildren(root, path, false);
                }

                if (UnityEditor.SceneManagement.EditorSceneManager.sceneCount > 1)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Find Missing Scripts] Bỏ qua scene không mở được: '{path}'\n{e.Message}");
            }
        }

        // Khôi phục lại các scene đang mở trước khi bắt đầu quét
        if (originalSetup != null && originalSetup.Length > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
        }

        if (totalMissing == 0)
        {
            Debug.Log("<color=green>✔ Không tìm thấy Missing Script nào trong Assets.</color>");
        }
        else
        {
            Debug.LogWarning($"<color=red>⚠ Tổng cộng tìm thấy {totalMissing} Missing Script. Xem chi tiết ở các dòng phía trên.</color>");
        }
    }

    private static int CountMissingOnGameObjectAndChildren(GameObject go, string assetPath, bool isPrefab)
    {
        int count = 0;
        Component[] components = go.GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                count++;
                Debug.LogError(
                    $"[MISSING SCRIPT] {(isPrefab ? "Prefab" : "Scene")}: '{assetPath}'  →  GameObject: '{GetFullPath(go)}'",
                    go);
            }
        }

        foreach (Transform child in go.transform)
        {
            count += CountMissingOnGameObjectAndChildren(child.gameObject, assetPath, isPrefab);
        }

        return count;
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
}