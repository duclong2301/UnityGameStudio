using CubeMergeArena.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CubeMergeArenaGameplayBuilder
{
    private const string DataFolder = "Assets/_Project/Data";
    private const string MaterialFolder = "Assets/_Project/Materials/CubeMergeArena";
    private const string GameplayPrefabFolder = "Assets/Prefabs/CubeMergeArena/Gameplay";
    private const string UiPrefabFolder = "Assets/Prefabs/UI/CubeMergeArena";
    private const string BalancePath = DataFolder + "/CubeMergeArenaBalance.asset";
    private const string MainMenuPrefabPath = UiPrefabFolder + "/CubeMergeArenaMainMenu.prefab";

    [MenuItem("Tools/Cube Merge Arena/Rebuild Gameplay Prefab Scene")]
    [MenuItem("Tools/Cube Merge Arena/Rebuild Gameplay Prototype")]
    public static void BuildFromMenu()
    {
        var balance = LoadOrCreateBalance();
        var assets = CreateOrUpdatePrefabAssets(balance);
        RebuildScene(assets);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Cube Merge Arena scene rebuilt with prefab instances and serialized gameplay/UI references.");
    }

    private static GameplayPrefabAssets CreateOrUpdatePrefabAssets(CubeMergeArenaBalance balance)
    {
        EnsureFolder("Assets", "_Project");
        EnsureFolder("Assets/_Project", "Materials");
        EnsureFolder("Assets/_Project/Materials", "CubeMergeArena");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets/Prefabs", "CubeMergeArena");
        EnsureFolder("Assets/Prefabs/CubeMergeArena", "Gameplay");

        var cubeMaterial = CreateMaterial("CubeMergeArenaCube.mat", new Color32(47, 152, 255, 255));
        var groundMaterial = CreateMaterial("CubeMergeArenaGround.mat", new Color32(0, 94, 210, 255));

        var segmentPrefabObject = CreateSegmentPrefab(cubeMaterial);
        var pickupPrefabObject = CreatePickupPrefab(cubeMaterial);
        var segmentPrefab = segmentPrefabObject.GetComponent<CubeSnakeSegment>();
        var pickupPrefab = pickupPrefabObject.GetComponent<CubeMergeArenaPickup>();
        var snakePrefab = CreateSnakePrefab(segmentPrefab);
        var spawnerPrefabObject = CreateSpawnerPrefab(pickupPrefab);
        var spawnerPrefab = spawnerPrefabObject.GetComponent<CubeMergeArenaSpawner>();
        var groundPrefab = CreateGroundPrefab(balance, groundMaterial);
        var cameraPrefab = CreateCameraPrefab();
        var lightPrefab = CreateLightPrefab();
        var gameplayPrefab = CreateGameplayPrefab(balance, groundPrefab, cameraPrefab, lightPrefab, snakePrefab, segmentPrefab, spawnerPrefab, pickupPrefab);
        var eventSystemPrefab = CreateEventSystemPrefab();

        return new GameplayPrefabAssets(gameplayPrefab, eventSystemPrefab);
    }

    private static GameObject CreateSegmentPrefab(Material material)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "SnakeSegment";
        root.GetComponent<Renderer>().sharedMaterial = material;

        var label = CreateWorldLabel("NumberLabel", root.transform);
        var segment = root.AddComponent<CubeSnakeSegment>();
        SetObjectReference(segment, "label", label);
        SetObjectReference(segment, "targetRenderer", root.GetComponent<Renderer>());

        return SavePrefab(root, GameplayPrefabFolder + "/SnakeSegment.prefab");
    }

    private static GameObject CreatePickupPrefab(Material material)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "Pickup";
        root.GetComponent<Renderer>().sharedMaterial = material;

        var label = CreateWorldLabel("Label", root.transform);
        var pickup = root.AddComponent<CubeMergeArenaPickup>();
        SetObjectReference(pickup, "label", label);
        SetObjectReference(pickup, "targetRenderer", root.GetComponent<Renderer>());

        return SavePrefab(root, GameplayPrefabFolder + "/Pickup.prefab");
    }

    private static GameObject CreateSnakePrefab(CubeSnakeSegment segmentPrefab)
    {
        var root = new GameObject("Snake");
        var snake = root.AddComponent<CubeSnake>();
        SetObjectReference(snake, "segmentPrefab", segmentPrefab);
        return SavePrefab(root, GameplayPrefabFolder + "/Snake.prefab");
    }

    private static GameObject CreateSpawnerPrefab(CubeMergeArenaPickup pickupPrefab)
    {
        var root = new GameObject("PickupSpawner");
        var spawner = root.AddComponent<CubeMergeArenaSpawner>();
        SetObjectReference(spawner, "pickupPrefab", pickupPrefab);
        return SavePrefab(root, GameplayPrefabFolder + "/PickupSpawner.prefab");
    }

    private static GameObject CreateGroundPrefab(CubeMergeArenaBalance balance, Material groundMaterial)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "ArenaGround";
        root.transform.position = new Vector3(0f, -0.05f, 0f);
        root.transform.localScale = new Vector3(balance.mapWidth, 0.1f, balance.mapHeight);
        root.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        return SavePrefab(root, GameplayPrefabFolder + "/ArenaGround.prefab");
    }

    private static GameObject CreateCameraPrefab()
    {
        var root = new GameObject("ArenaCamera");
        root.tag = "MainCamera";
        root.transform.position = new Vector3(0f, 62f, -48f);
        root.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

        var camera = root.AddComponent<Camera>();
        camera.fieldOfView = 45f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(0, 45, 140, 255);
        root.AddComponent<AudioListener>();
        root.AddComponent<CubeMergeArenaCameraFollow>();

        return SavePrefab(root, GameplayPrefabFolder + "/ArenaCamera.prefab");
    }

    private static GameObject CreateLightPrefab()
    {
        var root = new GameObject("ArenaLight");
        root.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var light = root.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;

        return SavePrefab(root, GameplayPrefabFolder + "/ArenaLight.prefab");
    }

    private static GameObject CreateGameplayPrefab(
        CubeMergeArenaBalance balance,
        GameObject groundPrefab,
        GameObject cameraPrefab,
        GameObject lightPrefab,
        GameObject snakePrefab,
        CubeSnakeSegment segmentPrefab,
        CubeMergeArenaSpawner spawnerPrefab,
        CubeMergeArenaPickup pickupPrefab)
    {
        var root = new GameObject("CubeMergeArenaGameplay");
        var runtimeContent = new GameObject("RuntimeContent");
        runtimeContent.transform.SetParent(root.transform, false);

        var ground = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab, root.transform);
        var cameraObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab, root.transform);
        var lightObject = (GameObject)PrefabUtility.InstantiatePrefab(lightPrefab, root.transform);

        var bootstrap = root.AddComponent<CubeMergeArenaRuntimeBootstrap>();
        SetObjectReference(bootstrap, "balance", balance);
        SetBool(bootstrap, "buildOnStart", false);
        SetBool(bootstrap, "clearExistingArenaOnBuild", true);
        SetObjectReference(bootstrap, "runtimeContentRoot", runtimeContent.transform);
        SetObjectReference(bootstrap, "ground", ground);
        SetObjectReference(bootstrap, "arenaCamera", cameraObject.GetComponent<Camera>());
        SetObjectReference(bootstrap, "arenaLight", lightObject.GetComponent<Light>());
        SetObjectReference(bootstrap, "snakePrefab", snakePrefab.GetComponent<CubeSnake>());
        SetObjectReference(bootstrap, "segmentPrefab", segmentPrefab);
        SetObjectReference(bootstrap, "spawnerPrefab", spawnerPrefab);
        SetObjectReference(bootstrap, "pickupPrefab", pickupPrefab);

        var flow = root.AddComponent<CubeMergeArenaGameFlow>();
        SetObjectReference(flow, "gameplayBootstrap", bootstrap);

        return SavePrefab(root, GameplayPrefabFolder + "/CubeMergeArenaGameplay.prefab");
    }

    private static GameObject CreateEventSystemPrefab()
    {
        var root = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return SavePrefab(root, GameplayPrefabFolder + "/EventSystem.prefab");
    }

    private static void RebuildScene(GameplayPrefabAssets assets)
    {
        DeleteRoot("Main Camera");
        DeleteRoot("EventSystem");
        DeleteRoot("CubeMergeArenaGameplay");

        var canvas = RebuildCanvasFromPrefab();
        var gameplay = (GameObject)PrefabUtility.InstantiatePrefab(assets.GameplayPrefab);
        gameplay.name = "CubeMergeArenaGameplay";

        var eventSystem = (GameObject)PrefabUtility.InstantiatePrefab(assets.EventSystemPrefab);
        eventSystem.name = "EventSystem";

        AssignFlowReferences(gameplay.GetComponent<CubeMergeArenaGameFlow>(), canvas);
    }

    private static GameObject RebuildCanvasFromPrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuPrefabPath);
        var existing = GameObject.Find("CubeMergeArenaCanvas");

        if (prefab == null)
        {
            return existing;
        }

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var canvas = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        canvas.name = "CubeMergeArenaCanvas";
        return canvas;
    }

    private static void AssignFlowReferences(CubeMergeArenaGameFlow flow, GameObject canvas)
    {
        if (flow == null || canvas == null)
        {
            return;
        }

        SetObjectReference(flow, "uiHome", FindPath(canvas.transform, "SafeArea/UIHome")?.gameObject);
        SetObjectReference(flow, "uiWin", FindPath(canvas.transform, "SafeArea/WinLayer/UIWin")?.gameObject);
        SetObjectReference(flow, "popupSetting", FindPath(canvas.transform, "SafeArea/PopupLayer/PopupSetting")?.gameObject);
        SetObjectReference(flow, "playButton", FindButton(canvas.transform, "SafeArea/UIHome/PrimaryActionLayer/PlayButton"));
        SetObjectReference(flow, "settingsButton", FindButton(canvas.transform, "SafeArea/UIHome/HeaderLayer/SettingsButton"));
        SetObjectReference(flow, "closeSettingsButton", FindButton(canvas.transform, "SafeArea/PopupLayer/PopupSetting/SettingsPanel/CloseButton"));
        SetObjectReference(flow, "closeSettingsBottomButton", FindButton(canvas.transform, "SafeArea/PopupLayer/PopupSetting/SettingsPanel/CloseBottomButton"));
        SetObjectReference(flow, "homeButton", FindButton(canvas.transform, "SafeArea/WinLayer/UIWin/ActionsLayer/HomeButton"));
        SetObjectReference(flow, "claimButton", FindButton(canvas.transform, "SafeArea/WinLayer/UIWin/ActionsLayer/ClaimButton"));
    }

    private static Button FindButton(Transform root, string path)
    {
        return FindPath(root, path)?.GetComponentInChildren<Button>(true);
    }

    private static Transform FindPath(Transform root, string path)
    {
        var current = root;
        var parts = path.Split('/');
        for (var i = 0; i < parts.Length; i++)
        {
            current = FindDirectChild(current, parts[i]);
            if (current == null)
            {
                return null;
            }
        }

        return current;
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static TextMeshPro CreateWorldLabel(string name, Transform parent)
    {
        var labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = new Vector3(0f, 0.56f, 0f);
        labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        labelObject.transform.localScale = Vector3.one * 0.12f;

        var label = labelObject.AddComponent<TextMeshPro>();
        label.fontSize = 8f;
        label.enableWordWrapping = false;
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        return label;
    }

    private static Material CreateMaterial(string fileName, Color color)
    {
        var path = MaterialFolder + "/" + fileName;
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(FindLitShader());
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader FindLitShader()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
        {
            return shader;
        }

        shader = Shader.Find("Standard");
        return shader != null ? shader : Shader.Find("Diffuse");
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
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

    private static void DeleteRoot(string objectName)
    {
        var target = GameObject.Find(objectName);
        if (target != null && target.transform.parent == null)
        {
            Object.DestroyImmediate(target);
        }
    }

    private static void SetObjectReference(Object target, string propertyName, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Object target, string propertyName, bool value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string parent, string name)
    {
        var path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private readonly struct GameplayPrefabAssets
    {
        public GameplayPrefabAssets(GameObject gameplayPrefab, GameObject eventSystemPrefab)
        {
            GameplayPrefab = gameplayPrefab;
            EventSystemPrefab = eventSystemPrefab;
        }

        public GameObject GameplayPrefab { get; }
        public GameObject EventSystemPrefab { get; }
    }
}
