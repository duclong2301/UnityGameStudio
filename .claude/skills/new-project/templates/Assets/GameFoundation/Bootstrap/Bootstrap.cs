using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap
{
 
    /// <summary>
    /// Modular bootstrap system.
    /// Configure modules directly in Inspector - add new systems without code changes.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class Bootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField] private BootstrapConfig config = new BootstrapConfig();

        [Header("Legacy Fallback (if config.modules empty)")]
        [SerializeField] private string legacyMainSceneName = "MainScene";
        [SerializeField] private float legacyMinLoadingSeconds = 1f;

        private BootstrapPipeline _pipeline;
        private BootstrapContext _context;

 
        public UnityEvent<float,string> OnProgress;
        private async void Start()
        {
            // If no modules configured, use legacy mode
            if (config == null || config.modules == null || config.modules.Count == 0)
            {
                Debug.LogWarning("[Bootstrap] No modules configured - using legacy mode");
                StartCoroutine(LoadMainLegacy());
                return;
            }

            try
            {
                await InitializeAsync();
                await LoadFirstSceneAsync();
                GameStateManager.Main();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Bootstrap] Critical error: {ex.Message}\n{ex.StackTrace}");
                // Could show error UI here
            }
        }

        /// <summary>
        /// Initialize all modules from config.
        /// </summary>
        private async Task InitializeAsync()
        {
            float startTime = Time.realtimeSinceStartup;

            // Create custom progress reporter
            var progressReporter = new Progress<float>(progress =>
            {
                UpdateProgress(progress, "Initializing modules...");
            });

            // Create context with progress reporter
            _context = new BootstrapContext(progressReporter);

            // Set callback for when module starts
            _context.OnModuleStarted = (moduleName, progress) =>
            {
                UpdateProgress(progress, $"Initializing {moduleName}...");
            };

            // Create pipeline
            _pipeline = new BootstrapPipeline(config);

            // Load and register modules from config
            foreach (var provider in config.modules)
            {
                if (!provider.isEnabled) continue;

                var module = provider.GetModule(transform);
                if (module != null)
                {
                    _pipeline.Register(module);
                }
                else
                {
                    Debug.LogWarning($"[Bootstrap] Failed to get module from provider");
                }
            }

            // Execute pipeline
            bool success = await _pipeline.ExecuteAsync(_context);

            if (!success)
            {
                throw new Exception("Bootstrap pipeline failed");
            }

            // Ensure minimum loading time
            float elapsed = Time.realtimeSinceStartup - startTime;
            float remaining = config.minLoadingTime - elapsed;
            if (remaining > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(remaining));
            }
        }

        private void UpdateProgress(float progress, string status)
        {
            OnProgress?.Invoke(progress, status);
        }


        /// <summary>
        /// Load first scene configured in BootstrapConfig.
        /// </summary>
        private async Task LoadFirstSceneAsync()
        {
            string sceneName = config.firstSceneName;
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[Bootstrap] No first scene configured");
                return;
            }

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!op.isDone)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        /// Legacy loading mode when no modules configured.
        /// </summary>
        private System.Collections.IEnumerator LoadMainLegacy()
        {
            yield return new WaitForSeconds(legacyMinLoadingSeconds);
            var op = SceneManager.LoadSceneAsync(legacyMainSceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
            GameStateManager.Main();
        }

        /// <summary>
        /// Access to bootstrap context for debugging.
        /// </summary>
        public BootstrapContext Context => _context;
    }
}
