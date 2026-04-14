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
using {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap;
using {{PROJECT_NAMESPACE}}.GameFoundation.UI;
using {{PROJECT_NAMESPACE}}.UI.Scenes;
using {{PROJECT_NAMESPACE}}.UI.Popups;
using {{PROJECT_NAMESPACE}}.Gameplay;

namespace {{PROJECT_NAMESPACE}}.EditorTools
{
    /// <summary>
    /// First-run scaffolder. On the first domain load after import, creates the three
    /// GameFoundation scenes (LoadingScene, MainScene, GameplayScene), registers them in
    /// EditorBuildSettings, imports TMP Essential Resources, and imports any .unitypackage
    /// files found in Assets/ImportQueue/. Safe to re-run — skips scenes that already exist.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectScaffolder
    {
        private const string ScenesDir     = "Assets/Scenes";
        private const string ImportQueue   = "Assets/ImportQueue";
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

            var loading  = EnsureScene(LoadingScene,  BuildLoadingScene);
            var main     = EnsureScene(MainScene,     BuildMainScene);
            var gameplay = EnsureScene(GameplayScene, BuildGameplayScene);

            RegisterScenes(loading, main, gameplay);
            AssetDatabase.SaveAssets();

            // Import TMP Essential Resources so TMP components work out of the box.
            // Must happen after scene save so AssetDatabase is in a clean state.
            EditorApplication.delayCall += ImportTMPEssentials;

            // Import any .unitypackage files dropped in Assets/ImportQueue/.
            EditorApplication.delayCall += ImportStartPacks;

            EditorPrefs.SetBool(MarkerKey, true);
            Debug.Log("[{{PROJECT_NAMESPACE}}] Starter scenes scaffolded.");
        }

        // ─── TMP Essential Resources ─────────────────────────────────────────

        private static void ImportTMPEssentials()
        {
            // Try the documented API first; fall back to the menu item if unavailable.
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

            // Fallback: trigger via menu item (opens a progress dialog but is reliable).
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
                // ImportPackage path must be relative to the project root or absolute.
                var absPath = Path.GetFullPath(pkg);
                AssetDatabase.ImportPackage(absPath, false); // false = non-interactive
                Debug.Log($"[{{PROJECT_NAMESPACE}}] Imported start-pack: {Path.GetFileName(pkg)}");
            }

            AssetDatabase.Refresh();
        }

        // ─── Scene builders ───────────────────────────────────────────────────

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
            AddFullscreenImage(ui.transform, new Color(0.05f, 0.05f, 0.1f, 1f));
            AddLabel(ui.transform, "LoadingLabel", "Loading...", 48, new Vector2(0, 60));

            var bootstrap = new GameObject("[SystemBootstrap]", typeof(Bootstrap));
            bootstrap.GetComponent<Bootstrap>();

            // UIManager + Popup registry lives here so it persists across scenes.
            var uiMgr = new GameObject("[UIManager]", typeof(UIManager));
            var popupRoot = new GameObject("PopupSettings", typeof(RectTransform), typeof(PopupSettings));
            popupRoot.transform.SetParent(uiMgr.transform, false);
            AddFullscreenImage(popupRoot.transform, new Color(0f, 0f, 0f, 0.7f));
            AddLabel(popupRoot.transform, "Title", "Settings", 42, new Vector2(0, 120));
            var closeBtn = AddButton(popupRoot.transform, "CloseButton", "Close", new Vector2(0, -80));
            var popup = popupRoot.GetComponent<PopupSettings>();
            var closeField = typeof(PopupSettings).GetField("closeButton",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            closeField?.SetValue(popup, closeBtn);
        }

        private static void BuildMainScene(UnityEngine.SceneManagement.Scene scene)
        {
            CreateEventSystem();
            var canvas = CreateCanvas("MainCanvas");
            var ui = new GameObject("UIMainScene", typeof(RectTransform), typeof(UIMainScene));
            ui.transform.SetParent(canvas.transform, false);
            AddFullscreenImage(ui.transform, new Color(0.15f, 0.15f, 0.2f, 1f));
            AddLabel(ui.transform, "Title", "MAIN MENU", 72, new Vector2(0, 180));

            var playBtn     = AddButton(ui.transform, "PlayButton",     "Play",     new Vector2(0,    0));
            var settingsBtn = AddButton(ui.transform, "SettingsButton", "Settings", new Vector2(0, -140));

            var sceneComp = ui.GetComponent<UIMainScene>();
            var t = typeof(UIMainScene);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            t.GetField("playButton",     flags)?.SetValue(sceneComp, playBtn);
            t.GetField("settingsButton", flags)?.SetValue(sceneComp, settingsBtn);
        }

        private static void BuildGameplayScene(UnityEngine.SceneManagement.Scene scene)
        {
            CreateEventSystem();
            var canvas = CreateCanvas("GameplayCanvas");
            var ui = new GameObject("UIGameplayScene", typeof(RectTransform), typeof(UIGameplayScene));
            ui.transform.SetParent(canvas.transform, false);
            AddFullscreenImage(ui.transform, new Color(0.1f, 0.2f, 0.1f, 1f));
            var label = AddLabel(ui.transform, "StateLabel", "READY", 120, Vector2.zero);

            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(UIGameplayScene)
                .GetField("stateLabel", flags)
                ?.SetValue(ui.GetComponent<UIGameplayScene>(), label);

            new GameObject("[GameplayController]", typeof(GameplayController));
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static GameObject CreateEventSystem()
        {
            return new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
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

        private static void AddFullscreenImage(Transform parent, Color color)
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
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

        private static Button AddButton(Transform parent, string name, string label,
            Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(360, 100);
            rt.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.8f, 1f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 42;
            tmp.alignment = TextAlignmentOptions.Center;

            return go.GetComponent<Button>();
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
