using CubeMergeArena.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CubeMergeArenaGameplayBuilder
{
    private const string DataFolder = "Assets/_Project/Data";
    private const string BalancePath = DataFolder + "/CubeMergeArenaBalance.asset";

    [MenuItem("Tools/Cube Merge Arena/Rebuild Gameplay Prototype")]
    public static void BuildFromMenu()
    {
        var balance = LoadOrCreateBalance();
        var existing = GameObject.Find("CubeMergeArenaGameplay");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var root = new GameObject("CubeMergeArenaGameplay");
        var bootstrap = root.AddComponent<CubeMergeArenaRuntimeBootstrap>();
        var serialized = new SerializedObject(bootstrap);
        serialized.FindProperty("balance").objectReferenceValue = balance;
        serialized.FindProperty("buildOnStart").boolValue = false;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        root.AddComponent<CubeMergeArenaGameFlow>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Cube Merge Arena gameplay prototype created. Press Play, then click the UI PLAY button to spawn player, bots, pickups and boosters.");
    }

    private static CubeMergeArenaBalance LoadOrCreateBalance()
    {
        EnsureFolder("Assets", "_Project");
        EnsureFolder("Assets/_Project", "Data");

        var balance = AssetDatabase.LoadAssetAtPath<CubeMergeArenaBalance>(BalancePath);
        if (balance != null)
        {
            return balance;
        }

        balance = ScriptableObject.CreateInstance<CubeMergeArenaBalance>();
        AssetDatabase.CreateAsset(balance, BalancePath);
        AssetDatabase.SaveAssets();
        return balance;
    }

    private static void EnsureFolder(string parent, string name)
    {
        var path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
