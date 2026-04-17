#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using [{{PROJECT_NAMESPACE}}].GameFoundation.Bootstrap;
using [{{PROJECT_NAMESPACE}}].GameFoundation.Data;
using [{{PROJECT_NAMESPACE}}].GameFoundation.UI;
using [{{PROJECT_NAMESPACE}}].GameFoundation.StateMachine;
using [{{PROJECT_NAMESPACE}}].GameFoundation;
using [{{PROJECT_NAMESPACE}}].UI.Scenes;
using [{{PROJECT_NAMESPACE}}].UI.Popups;
using [{{PROJECT_NAMESPACE}}].Gameplay;
using UnityEngine.Events;

namespace [{{PROJECT_NAMESPACE}}].EditorTools
{
    /// <summary>
    /// First-run scaffolder. On the first domain load after import, creates the three
    /// GameFoundation scenes (LoadingScene, MainScene, GameplayScene), registers them in
    /// EditorBuildSettings, imports TMP Essential Resources, and imports any .unitypackage
    /// files found in Assets/ImportQueue/. Safe to re-run — skips scenes that already exist.
    ///
    /// Scene contracts (verified against the live PuzzleBallSort project hierarchy):
    ///
    ///   LoadingScene
    ///   ├── EventSystem
    ///   ├── LoadingCanvas
    ///   │   └── UILoadingScene  (UIBase, Layer=Loading)
    ///   │       ├── Background  (Image, dark)
    ///   │       ├── LoadingLabel (TMP "Loading...") — wired to statusLabel
    ///   │       └── Slider       (Unity default UI slider) — wired to progressBar
    ///   │           ├── Background
    ///   │           ├── Fill Area
    ///   │           │   └── Fill
    ///   │           └── Handle Slide Area
    ///   │               └── Handle
    ///   ├── [SystemBootstrap]  (Bootstrap)
    ///   ├── Camera             (default Unity camera)
    ///   └── DataManager        (singleton — Bootstrap finds via FindObjectOfType)
    ///
    ///   MainScene
    ///   └── Camera             (intentionally minimal — game-specific UI lives here)
    ///
    ///   GameplayScene
    ///   ├── EventSystem
    ///   ├── [GameplayController]
    ///   └── Camera             (intentionally minimal — game-specific UI lives here)
    ///
    /// MainScene/GameplayScene UI is intentionally NOT scaffolded; teams add scene-specific
    /// content. The Bootstrap pipeline drives state transitions regardless of UI presence.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectScaffolder
    {
        private const string ScenesDir     = "Assets/Scenes";
        private const string ImportQueue   = "Assets/ImportQueue";
        private const string UIManagerDir  = "Assets/GameFoundation/UI";
        private const string UIManagerPrefabPath = "Assets/GameFoundation/UI/[UIManager].prefab";
        private const string MusicManagerDir  = "Assets/GameFoundation/MusicManager";
        private const string SoundManagerDir  = "Assets/GameFoundation/SoundManager";
        private const string VibrationDir     = "Assets/GameFoundation/Vibration";
        private const string LoadingScene  = "LoadingScene";
        private const string MainScene     = "MainScene";
        private const string GameplayScene = "GameplayScene";
        private const string MarkerKey     = "{{PROJECT_NAMESPACE}}.Scaffolded";

        static ProjectScaffolder()
        {
            if (EditorPrefs.GetBool(MarkerKey, false)) return;
            EditorApplication.delayCall += RunOnce;
        }

        [MenuItem("{{PROJECT_NAMESPACE}}/Rebuild Starter Scenes")]
        private static void RebuildMenu()
        {
            EditorPrefs.DeleteKey(MarkerKey);
            RunOnce();
        }

        private static void RunOnce()
        {
            if (!Directory.Exists(ScenesDir)) Directory.CreateDirectory(ScenesDir);
            if (!Directory.Exists(UIManagerDir)) Directory.CreateDirectory(UIManagerDir);
            if (!Directory.Exists(MusicManagerDir)) Directory.CreateDirectory(MusicManagerDir);
            if (!Directory.Exists(SoundManagerDir)) Directory.CreateDirectory(SoundManagerDir);
            if (!Directory.Exists(VibrationDir)) Directory.CreateDirectory(VibrationDir);

            // Create UIManager prefab first (needed for Bootstrap config)
            CreateUIManagerPrefab();

            var loading  = EnsureScene(LoadingScene,  BuildLoadingScene);
            var main     = EnsureScene(MainScene,     BuildMainScene);
            var gameplay = EnsureScene(GameplayScene, BuildGameplayScene);

            RegisterScenes(loading, main, gameplay);
            AssetDatabase.SaveAssets();

            EditorApplication.delayCall += ImportTMPEssentials;
            EditorApplication.delayCall += ImportStartPacks;

            EditorPrefs.SetBool(MarkerKey, true);
            Debug.Log("[{{PROJECT_NAMESPACE}}] Starter scenes scaffolded.");
        }

        // ─── TMP Essential Resources ─────────────────────────────────────────

        private static void ImportTMPEssentials()
        {
            var utilType = System.Type.GetType(
                "TMPro.TMP_PackageUtilities, Unity.TextMeshPro.Editor");
            if (utilType != null)
            {
                var method = utilType.GetMethod(
                    "ImportProjectResourcesMenu",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (method != null)
                {
                    method.Invoke(null, null);
                    Debug.Log("[{{PROJECT_NAMESPACE}}] TMP Essential Resources imported via API.");
                    return;
                }
            }

            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            Debug.Log("[{{PROJECT_NAMESPACE}}] TMP Essential Resources import triggered via menu.");
        }

        // ─── Start-pack auto-import ──────────────────────────────────────────

        private static void ImportStartPacks()
        {
            if (!Directory.Exists(ImportQueue)) return;

            var packages = Directory.GetFiles(ImportQueue, "*.unitypackage",
                SearchOption.TopDirectoryOnly);

            if (packages.Length == 0) return;

            foreach (var pkg in packages)
            {
                var absPath = Path.GetFullPath(pkg);
                AssetDatabase.ImportPackage(absPath, false);
                Debug.Log($"[{{PROJECT_NAMESPACE}}] Imported start-pack: {Path.GetFileName(pkg)}");
            }

            AssetDatabase.Refresh();
        }

        // ─── Scene builders ───────────────────────────────────────────────────

        /// <summary>
        /// Creates [UIManager] prefab with UIManager component and proper hierarchy.
        /// Structure:
        ///   [UIManager]  (UIManager component)
        ///   ├── Scenes   (Canvas for Scene layer)
        ///   │   ├── UIMainScene (with buttons wired)
        ///   │   └── UIGameplayScene (with label/button wired)
        ///   ├── Popups   (Canvas for Popup layer)
        ///   │   └── PopupSettings (with close button wired)
        ///   └── EventSystem
        /// </summary>
        [MenuItem("[{{PROJECT_NAMESPACE}}]/Create UIManager Prefab")]
        private static void CreateUIManagerPrefab()
        {
            if (File.Exists(UIManagerPrefabPath))
            {
                Debug.Log($"[{{PROJECT_NAMESPACE}}] UIManager prefab already exists, skipping.");
                return;
            }

            // Root GameObject with UIManager component
            var uiManagerGo = new GameObject("[UIManager]", typeof(UIManager));
            
            // Scenes Canvas (Layer 0 - sort order 0-99)
            var scenesCanvas = CreateUICanvas(uiManagerGo.transform, "Scenes", 0, 0);
            
            // Create UIMainScene
            var mainScene = CreateUIMainScene(scenesCanvas.transform);
            
            // Create UIGameplayScene
            var gameplayScene = CreateUIGameplayScene(scenesCanvas.transform);
            
            // Popups Canvas (Layer 1 - sort order 100-199)
            var popupsCanvas = CreateUICanvas(uiManagerGo.transform, "Popups", 1, 100);
            
            // Create PopupSettings
            var popupSettings = CreatePopupSettings(popupsCanvas.transform);
            
            // EventSystem
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(uiManagerGo.transform, false);
            
            // Wire UIManager state scene mappings
            var uiManager = uiManagerGo.GetComponent<UIManager>();
            var mappingsField = typeof(UIManager).GetField("stateSceneMappings",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (mappingsField != null)
            {
                var mappings = new List<StateSceneMapping>
                {
                    new StateSceneMapping 
                    { 
                        state = GameState.Main, 
                        sceneUI = mainScene 
                    },
                    new StateSceneMapping 
                    { 
                        state = GameState.Play, 
                        sceneUI = gameplayScene 
                    }
                };
                mappingsField.SetValue(uiManager, mappings);
            }
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(uiManagerGo, UIManagerPrefabPath);
            Object.DestroyImmediate(uiManagerGo);
            
            Debug.Log($"[{{PROJECT_NAMESPACE}}] Created UIManager prefab at {UIManagerPrefabPath}");
        }

        /// <summary>
        /// Creates a Canvas for UI layers with proper settings.
        /// </summary>
        private static Canvas CreateUICanvas(Transform parent, string name, int layer, int sortOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), 
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            
            go.transform.SetParent(parent, false);
            go.layer = 5; // UI layer
            
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            return canvas;
        }

        /// <summary>
        /// Creates UIMainScene with playButton and settingsButton wired.
        /// </summary>
        private static UISceneBase CreateUIMainScene(Transform parent)
        {
            var go = new GameObject("UIMainScene", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            StretchFull(rt);
            go.layer = 5;

            // Background
            AddFullscreenImage(go.transform, new Color(0.05f, 0.05f, 0.1f, 1f));

            // Title
            AddLabel(go.transform, "Title", "MAIN MENU", 72, new Vector2(0, 180));

            // Play Button
            var playButton = CreateUIButton(go.transform, "PlayButton", "Play", new Vector2(0, 0));

            // Settings Button
            var settingsButton = CreateUIButton(go.transform, "SettingsButton", "Settings", new Vector2(0, -120));

            // Add UIMainScene script and wire references
            var sceneScript = go.AddComponent<UIMainScene>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(UIMainScene).GetField("playButton", flags)?.SetValue(sceneScript, playButton);
            typeof(UIMainScene).GetField("settingsButton", flags)?.SetValue(sceneScript, settingsButton);
            typeof(UIMainScene).GetField("gameplaySceneName", flags)?.SetValue(sceneScript, "GameplayScene");

            return sceneScript;
        }

        /// <summary>
        /// Creates UIGameplayScene with stateLabel and homeButton wired.
        /// </summary>
        private static UISceneBase CreateUIGameplayScene(Transform parent)
        {
            var go = new GameObject("UIGameplayScene", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            StretchFull(rt);
            go.layer = 5;

            // Background
            AddFullscreenImage(go.transform, new Color(0.15f, 0.15f, 0.2f, 1f));

            // State Label
            var stateLabel = AddLabel(go.transform, "StateLabel", "READY", 48, new Vector2(0, 300));

            // Home Button (top-left)
            var homeButton = CreateUIButton(go.transform, "HomeButton", "← Home", new Vector2(0, 450));
            var homeRt = (RectTransform)homeButton.transform;
            homeRt.sizeDelta = new Vector2(200, 80);

            // Add UIGameplayScene script and wire references
            var sceneScript = go.AddComponent<UIGameplayScene>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(UIGameplayScene).GetField("stateLabel", flags)?.SetValue(sceneScript, stateLabel);
            typeof(UIGameplayScene).GetField("homeButton", flags)?.SetValue(sceneScript, homeButton);

            return sceneScript;
        }

        /// <summary>
        /// Creates PopupSettings with closeButton wired.
        /// </summary>
        private static UIPopupBase CreatePopupSettings(Transform parent)
        {
            var go = new GameObject("PopupSettings", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            StretchFull(rt);
            go.layer = 5;

            // Semi-transparent background overlay
            AddFullscreenImage(go.transform, new Color(0, 0, 0, 0.7f));

            // Panel container
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(go.transform, false);
            panel.layer = 5;
            var panelRt = (RectTransform)panel.transform;
            panelRt.sizeDelta = new Vector2(600, 400);
            panelRt.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Title
            var title = AddLabel(panel.transform, "Title", "SETTINGS", 48, new Vector2(0, 140));

            // Close Button
            var closeButton = CreateUIButton(panel.transform, "CloseButton", "Close", new Vector2(0, -120));

            // Add PopupSettings script and wire references
            var popupScript = go.AddComponent<PopupSettings>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(PopupSettings).GetField("closeButton", flags)?.SetValue(popupScript, closeButton);

            return popupScript;
        }

        /// <summary>
        /// Creates a UI button with text label.
        /// </summary>
        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.layer = 5;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(360, 100);
            rt.anchoredPosition = anchoredPos;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.25f, 0.45f, 0.8f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = img;

            // Label
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            labelGo.layer = 5;
            var labelRt = (RectTransform)labelGo.transform;
            StretchFull(labelRt);

            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 42;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return button;
        }

        private static string EnsureScene(string name,
            System.Action<UnityEngine.SceneManagement.Scene> builder)
        {
            var path = $"{ScenesDir}/{name}.unity";
            if (File.Exists(path)) return path;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            builder(scene);
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void BuildLoadingScene(UnityEngine.SceneManagement.Scene scene)
        {
            CreateEventSystem();

            var canvas = CreateCanvas("LoadingCanvas");
            var ui = new GameObject("UILoadingScene", typeof(RectTransform), typeof(UILoadingScene));
            ui.transform.SetParent(canvas.transform, false);
            StretchFull((RectTransform)ui.transform);
            AddFullscreenImage(ui.transform, new Color(0.05f, 0.05f, 0.1f, 1f));

            var statusLabel = AddLabel(ui.transform, "LoadingLabel", "Loading...", 48, new Vector2(0, 60));
            var slider      = CreateProgressSlider(ui.transform, new Vector2(0, -120), new Vector2(800, 40));

            var sceneComp = ui.GetComponent<UILoadingScene>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(UILoadingScene).GetField("progressBar", flags)?.SetValue(sceneComp, slider);
            typeof(UILoadingScene).GetField("statusLabel", flags)?.SetValue(sceneComp, statusLabel);

            // Create Bootstrap and wire OnProgress to UILoadingScene.SetProgress
            var bootstrapGo = new GameObject("[SystemBootstrap]", typeof(Bootstrap));
            var bootstrap = bootstrapGo.GetComponent<Bootstrap>();
            
            // Wire up OnProgress event
            if (bootstrap.OnProgress == null)
                bootstrap.OnProgress = new UnityEvent<float, string>();
            
            UnityEditor.Events.UnityEventTools.AddPersistentListener(bootstrap.OnProgress, sceneComp.SetProgress);

            // Configure Bootstrap modules
            ConfigureBootstrapModules(bootstrap);

            CreateCamera("Camera");

            // Singleton audio/haptic managers — live in LoadingScene, survive via DontDestroyOnLoad.
            // MusicManager calls DontDestroyOnLoad(gameObject) in Awake.
            var musicManagerGo = new GameObject("MusicManager");
            musicManagerGo.AddComponent<MusicManager>(); // AudioSource added by RequireComponent

            var soundManagerGo = new GameObject("SoundManager");
            soundManagerGo.AddComponent<SoundManager>(); // AudioSource added by RequireComponent

            new GameObject("HapticFeedback", typeof(HapticFeedback));

            // DataManager singleton lives here so Bootstrap's FindObjectOfType picks it up.
            // See Assets/GameFoundation/Bootstrap/SINGLETON_MANAGERS.md.
   
        }

        /// <summary>
        /// Configures Bootstrap with UIManager prefab and DataManager (use existing instance).
        /// </summary>
        private static void ConfigureBootstrapModules(Bootstrap bootstrap)
        {
            // Access the config field
            var configField = typeof(Bootstrap).GetField("config",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (configField == null)
            {
                Debug.LogError("[{{PROJECT_NAMESPACE}}] Could not find Bootstrap.config field!");
                return;
            }

            var config = (BootstrapConfig)configField.GetValue(bootstrap);
            if (config == null)
            {
                config = new BootstrapConfig();
                configField.SetValue(bootstrap, config);
            }

            // Load UIManager prefab
            var uiManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UIManagerPrefabPath);
            if (uiManagerPrefab == null)
            {
                Debug.LogWarning($"[{{PROJECT_NAMESPACE}}] UIManager prefab not found at {UIManagerPrefabPath}");
                return;
            }

            // Clear existing modules and add new ones
            config.modules.Clear();

            // Module 0: UIManager (from prefab)
            config.modules.Add(new ModuleProvider
            {
                moduleAsset = uiManagerPrefab,
                isRequired = true,
                isEnabled = true,
                useExistingInstance = false
            });

            // Module 1: DataManager (use existing instance in scene)
            // We need a reference to a DataManager component for the moduleAsset
            // Create a temporary one to get the component type
            var tempDM = new GameObject("DataManager", typeof(DataManager));
            
            config.modules.Add(new ModuleProvider
            {
                moduleAsset = tempDM,
                isRequired = true,
                isEnabled = true,
                useExistingInstance = true
            });
            
            //Object.DestroyImmediate(tempDM);

            Debug.Log("[{{PROJECT_NAMESPACE}}] Bootstrap modules configured: UIManager (prefab), DataManager (existing instance)");
        }

        private static void BuildMainScene(UnityEngine.SceneManagement.Scene scene)
        {
            // MainScene is intentionally minimal — game-specific menu UI is added by the team.
            CreateCamera("Camera");
        }

        private static void BuildGameplayScene(UnityEngine.SceneManagement.Scene scene)
        {
            CreateEventSystem();
            new GameObject("[GameplayController]", typeof(GameplayController));
            CreateCamera("Camera");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static GameObject CreateEventSystem()
        {
            return new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static Camera CreateCamera(string name)
        {
            var go = new GameObject(name, typeof(Camera));
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.192f, 0.302f, 0.475f, 0f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = 60f;
            return cam;
        }

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void AddFullscreenImage(Transform parent, Color color)
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            StretchFull((RectTransform)go.transform);
            go.GetComponent<Image>().color = color;
        }

        private static TMP_Text AddLabel(Transform parent, string name, string text,
            float size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1200, 200);
            rt.anchoredPosition = anchoredPos;
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            return t;
        }

        // Builds a Unity-standard UI Slider hierarchy:
        //   Slider [Slider+Image]
        //   ├── Background           [Image]
        //   ├── Fill Area            [RectTransform]
        //   │   └── Fill             [Image]
        //   └── Handle Slide Area    [RectTransform]
        //       └── Handle           [Image]
        private static Slider CreateProgressSlider(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Image), typeof(Slider));
            sliderGo.transform.SetParent(parent, false);
            var sliderRt = (RectTransform)sliderGo.transform;
            sliderRt.sizeDelta = size;
            sliderRt.anchoredPosition = anchoredPos;
            sliderGo.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGo.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0f, 0.25f); bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var faRt = (RectTransform)fillArea.transform;
            faRt.anchorMin = new Vector2(0f, 0.25f); faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.offsetMin = new Vector2(5f, 0f);   faRt.offsetMax = new Vector2(-15f, 0f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(-5f, 0f); fillRt.offsetMax = new Vector2(5f, 0f);
            fill.GetComponent<Image>().color = new Color(0.25f, 0.6f, 0.9f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            var haRt = (RectTransform)handleArea.transform;
            haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
            haRt.offsetMin = new Vector2(10f, 0f); haRt.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRt = (RectTransform)handle.transform;
            handleRt.sizeDelta = new Vector2(20f, 0f);
            handle.GetComponent<Image>().color = Color.white;

            var slider = sliderGo.GetComponent<Slider>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect      = fillRt;
            slider.handleRect    = handleRt;
            slider.direction     = Slider.Direction.LeftToRight;
            slider.minValue      = 0f;
            slider.maxValue      = 1f;
            slider.value         = 0f;
            return slider;
        }

        private static void RegisterScenes(params string[] scenePaths)
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var p in scenePaths)
                if (!string.IsNullOrEmpty(p))
                    list.Add(new EditorBuildSettingsScene(p, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
#endif
