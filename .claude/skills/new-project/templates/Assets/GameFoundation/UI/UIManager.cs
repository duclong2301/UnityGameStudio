using System;
using System.Collections.Generic;
using UnityEngine;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;
using {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap;
using System.Threading.Tasks;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.UI
{
    /// <summary>
    /// Maps a GameState to its corresponding UIScene type.
    /// </summary>
    [Serializable]
    public class StateSceneMapping
    {
        public GameState state;
        public UISceneBase sceneUI;
    }

    /// <summary>
    /// Orchestrates UI layers. Listens to <see cref="GameStateManager.OnGameStateChanged"/>
    /// and swaps the active Scene Layer. Other layers are callable on-demand.
    /// </summary>
    public class UIManager : MonoBehaviour, IBootstrapModule
    {
        public static UIManager Instance { get; private set; }

#region  IBootstrapModule Implementation
        public string ModuleName => "UIManager";

        public int Priority =>  20; // After DataManager

        string[] IBootstrapModule.Dependencies => null;

        public Task<bool> InitializeAsync(BootstrapContext context)
        {
            Debug.Log("[UIManager] Initialization complete.");

            foreach (var ui in GetComponentsInChildren<UIBase>(true))
            {
                _registry[ui.GetType()] = ui;
                ui.gameObject.SetActive(false);
            }

            // Build state → scene lookup table
            _stateToSceneMap.Clear();
            foreach (var mapping in stateSceneMappings)
            {
                if (mapping.sceneUI != null)
                    _stateToSceneMap[mapping.state] = mapping.sceneUI;
            }

            return Task.FromResult(true);
        }
#endregion

        [Header("State → Scene Mapping")]
        [Tooltip("Configure which UI scene appears for each GameState")]
        [SerializeField] private List<StateSceneMapping> stateSceneMappings = new();

        private readonly Dictionary<System.Type, UIBase> _registry = new();
        private readonly Dictionary<GameState, UISceneBase> _stateToSceneMap = new();
        private UISceneBase _activeScene;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() => GameStateManager.OnGameStateChanged += HandleStateChanged;
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
            // Auto-switch scene based on state → scene mapping
            if (_stateToSceneMap.TryGetValue(current, out var targetScene))
            {
                if (_activeScene != targetScene)
                {
                    HideAllPopups();
                    if (_activeScene != null) _activeScene.HideInternal();
                    _activeScene = targetScene;
                    _activeScene.ShowInternal(data);
                    Debug.Log($"[UIManager] State {current} → {targetScene.GetType().Name}");
                }
            }
            else
            {
                Debug.LogWarning($"[UIManager] No UI scene mapped for state: {current}");
            }
        }

        public void SetActiveScene<T>(object data = null) where T : UISceneBase
        {
            var next = Get<T>();
            if (next == null) return;
            if (_activeScene != null && _activeScene != next) _activeScene.HideInternal();
            _activeScene = next;
            next.ShowInternal(data);
        }

        public void HideAllPopups()
        {
            foreach (var ui in _registry.Values)
            {
                if (ui.Layer == UILayer.Popup) ui.HideInternal();
            }
        }

        public void HideActiveScene()
        {
            _activeScene?.HideInternal();
            _activeScene = null;
        }

        public void HideAllUI()
        {
            HideActiveScene();
            HideAllPopups();
        }

    }
}
