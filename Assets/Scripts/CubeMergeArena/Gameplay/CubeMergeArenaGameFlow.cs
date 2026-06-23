using UnityEngine;
using UnityEngine.UI;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeMergeArenaGameFlow : MonoBehaviour
    {
        public enum GameState
        {
            None,
            Init,
            Home,
            Playing,
            Win,
            Settings
        }

        [SerializeField] private CubeMergeArenaRuntimeBootstrap gameplayBootstrap;
        [SerializeField] private GameObject uiHome;
        [SerializeField] private GameObject uiWin;
        [SerializeField] private GameObject popupSetting;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button closeSettingsBottomButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button claimButton;
        [SerializeField] private bool showHomeOnStart = true;

        [SerializeField] private GameState currentState = GameState.None;

        private GameState stateBeforeSettings = GameState.Home;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            currentState = GameState.None;
            ChangeState(GameState.Init);
        }

        public void StartGame()
        {
            if (gameplayBootstrap == null) return;

            ChangeState(GameState.Playing);
        }

        public void ShowHome()
        {
            ChangeState(GameState.Home);
        }

        public void ShowWin()
        {
            if (!IsGameActive()) return;

            ChangeState(GameState.Win);
        }

        public void OpenSettings()
        {
            ChangeState(GameState.Settings);
        }

        public void CloseSettings()
        {
            ChangeState(stateBeforeSettings);
        }

        public void ChangeState(GameState nextState)
        {
            if (currentState == nextState) return;

            var previousState = currentState;
            currentState = nextState;
            EnterState(nextState, previousState);
        }

        private void EnterState(GameState state, GameState previousState)
        {
            switch (state)
            {
                case GameState.Init:
                    Init();
                    break;
                case GameState.Home:
                    EnterHome();
                    break;
                case GameState.Playing:
                    EnterPlaying();
                    break;
                case GameState.Win:
                    EnterWin();
                    break;
                case GameState.Settings:
                    EnterSettings(previousState);
                    break;
            }
        }

        private void Init()
        {
            ResolveReferences();
            BindButtons();

            if (showHomeOnStart)
            {
                ChangeState(GameState.Home);
            }
        }

        private void EnterHome()
        {
            if (gameplayBootstrap != null)
            {
                gameplayBootstrap.ClearArena();
            }

            SetActive(uiHome, true);
            SetActive(uiWin, false);
            SetActive(popupSetting, false);
        }

        private void EnterPlaying()
        {
            if (gameplayBootstrap == null) return;

            SetActive(uiHome, false);
            SetActive(uiWin, false);
            SetActive(popupSetting, false);
            gameplayBootstrap.BuildArena();
        }

        private void EnterWin()
        {
            SetActive(uiHome, false);
            SetActive(uiWin, true);
            SetActive(popupSetting, false);
        }

        private void EnterSettings(GameState previousState)
        {
            stateBeforeSettings = previousState == GameState.Settings ? stateBeforeSettings : previousState;
            SetActive(popupSetting, true);
        }

        private bool IsGameActive()
        {
            return currentState == GameState.Playing
                || currentState == GameState.Win
                || currentState == GameState.Settings && stateBeforeSettings == GameState.Playing;
        }

        private void ResolveReferences()
        {
            gameplayBootstrap = gameplayBootstrap != null ? gameplayBootstrap : GetComponent<CubeMergeArenaRuntimeBootstrap>();
        }

        private void BindButtons()
        {
            Bind(playButton, StartGame);
            Bind(settingsButton, OpenSettings);
            Bind(closeSettingsButton, CloseSettings);
            Bind(closeSettingsBottomButton, CloseSettings);
            Bind(homeButton, ShowHome);
            Bind(claimButton, ShowHome);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
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
