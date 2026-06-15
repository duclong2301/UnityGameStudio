using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeMergeArenaGameFlow : MonoBehaviour
    {
        [SerializeField] private CubeMergeArenaRuntimeBootstrap gameplayBootstrap;
        [SerializeField] private GameObject uiHome;
        [SerializeField] private GameObject uiWin;
        [SerializeField] private GameObject popupSetting;
        [SerializeField] private bool showHomeOnStart = true;

        private bool gameStarted;

        private void Awake()
        {
            ResolveReferences();
            BindButtons();

            if (showHomeOnStart)
            {
                ShowHome();
            }
        }

        public void StartGame()
        {
            if (gameplayBootstrap == null) return;

            gameStarted = true;
            SetActive(uiHome, false);
            SetActive(uiWin, false);
            SetActive(popupSetting, false);
            gameplayBootstrap.BuildArena();
        }

        public void ShowHome()
        {
            gameStarted = false;
            if (gameplayBootstrap != null)
            {
                gameplayBootstrap.ClearArena();
            }

            SetActive(uiHome, true);
            SetActive(uiWin, false);
            SetActive(popupSetting, false);
        }

        public void ShowWin()
        {
            if (!gameStarted) return;

            SetActive(uiHome, false);
            SetActive(uiWin, true);
            SetActive(popupSetting, false);
        }

        public void OpenSettings()
        {
            SetActive(popupSetting, true);
        }

        public void CloseSettings()
        {
            SetActive(popupSetting, false);
        }

        private void ResolveReferences()
        {
            gameplayBootstrap = gameplayBootstrap != null ? gameplayBootstrap : GetComponent<CubeMergeArenaRuntimeBootstrap>();
            uiHome = uiHome != null ? uiHome : FindSceneObject("CubeMergeArenaCanvas/SafeArea/UIHome");
            uiWin = uiWin != null ? uiWin : FindSceneObject("CubeMergeArenaCanvas/SafeArea/WinLayer/UIWin");
            popupSetting = popupSetting != null ? popupSetting : FindSceneObject("CubeMergeArenaCanvas/SafeArea/PopupLayer/PopupSetting");
        }

        private void BindButtons()
        {
            Bind("CubeMergeArenaCanvas/SafeArea/UIHome/PrimaryActionLayer/PlayButton", StartGame);
            Bind("CubeMergeArenaCanvas/SafeArea/UIHome/HeaderLayer/SettingsButton", OpenSettings);
            Bind("CubeMergeArenaCanvas/SafeArea/PopupLayer/PopupSetting/Content/CloseButton", CloseSettings);
            Bind("CubeMergeArenaCanvas/SafeArea/PopupLayer/PopupSetting/Content/CloseBottomButton", CloseSettings);
            Bind("CubeMergeArenaCanvas/SafeArea/WinLayer/UIWin/ActionsLayer/HomeButton", ShowHome);
            Bind("CubeMergeArenaCanvas/SafeArea/WinLayer/UIWin/ActionsLayer/ClaimButton", ShowHome);
        }

        private static void Bind(string path, UnityEngine.Events.UnityAction action)
        {
            var target = FindSceneObject(path);
            if (target == null) return;

            var button = target.GetComponentInChildren<Button>(true);
            if (button == null) return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static GameObject FindSceneObject(string path)
        {
            var parts = path.Split('/');
            if (parts.Length == 0) return null;

            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i].name != parts[0]) continue;

                var current = roots[i].transform;
                for (var p = 1; p < parts.Length; p++)
                {
                    current = FindDirectChild(current, parts[p]);
                    if (current == null) break;
                }

                if (current != null)
                {
                    return current.gameObject;
                }
            }

            return null;
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
