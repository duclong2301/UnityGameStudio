using System;
using System.Collections.Generic;
using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap
{
    /// <summary>
    /// Bootstrap configuration class.
    /// Serialized directly on Bootstrap MonoBehaviour - add/remove modules without code changes.
    /// </summary>
    [Serializable]
    public class BootstrapConfig
    {
        [Header("Module Configuration")]
        [Tooltip("List of module providers to initialize")]
        public List<ModuleProvider> modules = new List<ModuleProvider>();

        [Header("Scene Configuration")]
        [Tooltip("First scene to load after bootstrap")]
        public string firstSceneName = "MainScene";

        [Tooltip("Minimum loading duration (seconds)")]
        [Range(0f, 5f)]
        public float minLoadingTime = 1f;

        [Header("Debug Settings")]
        [Tooltip("Show detailed logs")]
        public bool verboseLogging = true;

        [Tooltip("Retry failed modules")]
        public bool retryOnFailure = false;

        [Tooltip("Max retry attempts")]
        [Range(1, 5)]
        public int maxRetries = 3;
    }

    /// <summary>
    /// Provides an IBootstrapModule instance.
    /// Can be a prefab (MonoBehaviour), ScriptableObject, or runtime-created.
    /// </summary>
    [Serializable]
    public class ModuleProvider
    {
        [Tooltip("Module asset (MonoBehaviour prefab or ScriptableObject)")]
        public UnityEngine.Object moduleAsset;

        [Tooltip("Required? Abort on failure if true")]
        public bool isRequired = true;

        [Tooltip("Enabled? Skip if false")]
        public bool isEnabled = true;

        [Tooltip("For MonoBehaviour: Use existing instance in scene if found (for singleton Managers)")]
        public bool useExistingInstance = true;

        /// <summary>
        /// Get the module instance from this provider.
        /// </summary>
        public IBootstrapModule GetModule(Transform parent)
        {
            if (moduleAsset == null) return null;

            // MonoBehaviour in prefab
            if (moduleAsset is GameObject prefab)
            {
                return GetMonoBehaviourModule(prefab, parent);
            }

            // Direct MonoBehaviour reference
            if (moduleAsset is MonoBehaviour mb)
            {
                return GetMonoBehaviourModule(mb, parent);
            }

            // ScriptableObject module
            if (moduleAsset is ScriptableObject so)
            {
                return so as IBootstrapModule;
            }

            Debug.LogError($"[ModuleProvider] Unsupported module type: {moduleAsset.GetType()}");
            return null;
        }

        /// <summary>
        /// Get MonoBehaviour module - checks for existing instance first if useExistingInstance is true.
        /// </summary>
        private IBootstrapModule GetMonoBehaviourModule(UnityEngine.Object asset, Transform parent)
        {
            // Determine the component type
            Type componentType = null;
            
            if (asset is GameObject go)
            {
                // Get IBootstrapModule component from prefab
                var prefabModule = go.GetComponent<IBootstrapModule>();
                if (prefabModule != null)
                {
                    componentType = prefabModule.GetType();
                }
            }
            else if (asset is MonoBehaviour mb)
            {
                componentType = mb.GetType();
            }

            if (componentType == null)
            {
                Debug.LogError($"[ModuleProvider] Could not determine component type for {asset.name}");
                return null;
            }

            // Check for existing instance in scene (for singleton Managers)
            if (useExistingInstance)
            {
                var existing = UnityEngine.Object.FindObjectOfType(componentType) as IBootstrapModule;
                if (existing != null)
                {
                    Debug.Log($"[ModuleProvider] Using existing {existing.ModuleName} instance from scene");
                    return existing;
                }
            }

            // No existing instance found or useExistingInstance is false - instantiate new one
            if (asset is GameObject prefab)
            {
                var instance = UnityEngine.Object.Instantiate(prefab);
                var module = instance.GetComponent<IBootstrapModule>();
                if (module != null)
                {
                    Debug.Log($"[ModuleProvider] Instantiated new {module.ModuleName} from prefab");
                }
                return module;
            }
            else if (asset is MonoBehaviour mbComp)
            {
                var instance = UnityEngine.Object.Instantiate(mbComp);
                var module = instance as IBootstrapModule;
                if (module != null)
                {
                    Debug.Log($"[ModuleProvider] Instantiated new {module.ModuleName} from component");
                }
                return module;
            }

            return null;
        }
    }
}
