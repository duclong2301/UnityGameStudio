using UnityEngine;
using TMPro;
using {{PROJECT_NAMESPACE}}.GameFoundation.UI;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;
using UnityEngine.UI;

namespace {{PROJECT_NAMESPACE}}.UI.Scenes
{
    public class UIGameplayScene : UISceneBase
    {
        [SerializeField] private TMP_Text stateLabel;
        [SerializeField] private Button homeButton;
        protected override void OnInitialize()
        {
            GameStateManager.OnGameStateChanged += HandleStateChanged;
            if (homeButton != null)
            {
                homeButton.onClick.AddListener(() =>
                {
                    GameStateManager.Main();
                });
            }
        }

        protected override void OnDispose()
        {
            GameStateManager.OnGameStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState current, GameState last, object data)
        {
            if (stateLabel == null) return;
            stateLabel.text = current switch
            {
                GameState.Init  => "LOADING...",
                GameState.Ready => "READY",
                GameState.Play  => "PLAY",
                _ => stateLabel.text,
            };
        }
    }
}
