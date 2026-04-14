using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using {{PROJECT_NAMESPACE}}.GameFoundation.UI;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;
using {{PROJECT_NAMESPACE}}.UI.Popups;

namespace {{PROJECT_NAMESPACE}}.UI.Scenes
{
    public class UIMainScene : UISceneBase
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private string gameplaySceneName = "GameplayScene";

        protected override void OnInitialize()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnPlayClicked()
        {
            GameStateManager.Ready();
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void OnSettingsClicked()
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowPopup<PopupSettings>();
        }
    }
}
