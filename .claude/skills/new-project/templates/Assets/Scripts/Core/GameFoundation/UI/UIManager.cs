using System.Collections.Generic;
using UnityEngine;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    /// <summary>
    /// Orchestrates UI layers. Listens to <see cref="GameStateManager.OnGameStateChanged"/>
    /// and swaps the active Scene Layer. Other layers are callable on-demand.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private readonly Dictionary<System.Type, UIBase> _registry = new();
        private UISceneBase _activeScene;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var ui in GetComponentsInChildren<UIBase>(true))
            {
                _registry[ui.GetType()] = ui;
                ui.gameObject.SetActive(false);
            }
        }

        private void OnEnable()  => GameStateManager.OnGameStateChanged += HandleStateChanged;
        private void OnDisable() => GameStateManager.OnGameStateChanged -= HandleStateChanged;

        public void Register(UIBase ui)
        {
            if (ui == null) return;
            _registry[ui.GetType()] = ui;
            if (ui.Layer != UILayer.Scene) ui.gameObject.SetActive(false);
        }

        public T Get<T>() where T : UIBase
        {
            if (_registry.TryGetValue(typeof(T), out var ui)) return (T)ui;
            return null;
        }

        public T ShowPopup<T>(object data = null) where T : UIPopupBase
        {
            var popup = Get<T>();
            if (popup == null) { Debug.LogWarning($"[UIManager] Popup {typeof(T).Name} not registered."); return null; }
            popup.Show(data);
            return popup;
        }

        public void HidePopup<T>() where T : UIPopupBase
        {
            var popup = Get<T>();
            popup?.Hide();
        }

        private void HandleStateChanged(GameState current, GameState last, object data)
        {
            // Scene transitions are driven by the active scene's bootstrapper —
            // UIManager only coordinates the visibility of registered Scene Layer UI.
            // Concrete scene selection happens in scene controllers (UILoadingScene, UIMainScene, UIGameplayScene).
        }

        public void SetActiveScene<T>(object data = null) where T : UISceneBase
        {
            var next = Get<T>();
            if (next == null) return;
            if (_activeScene != null && _activeScene != next) _activeScene.HideInternal();
            _activeScene = next;
            next.ShowInternal(data);
        }
    }
}
