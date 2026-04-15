using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap
{
    /// <summary>
    /// Bootstrap execution engine.
    /// Resolves dependencies, sorts modules, and executes initialization.
    /// </summary>
    public class BootstrapPipeline
    {
        private readonly List<IBootstrapModule> _modules = new List<IBootstrapModule>();
        private readonly Dictionary<string, IBootstrapModule> _moduleMap = new Dictionary<string, IBootstrapModule>();
        private readonly BootstrapConfig _config;

        public BootstrapPipeline(BootstrapConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>Register a module.</summary>
        public void Register(IBootstrapModule module)
        {
            if (module == null)
            {
                Debug.LogError("[Bootstrap] Cannot register null module");
                return;
            }

            if (_moduleMap.ContainsKey(module.ModuleName))
            {
                Debug.LogWarning($"[Bootstrap] Module {module.ModuleName} already registered");
                return;
            }

            _modules.Add(module);
            _moduleMap[module.ModuleName] = module;

            if (_config.verboseLogging)
                Debug.Log($"[Bootstrap] Registered: {module.ModuleName} (Priority: {module.Priority})");
        }

        /// <summary>Execute all modules in dependency order.</summary>
        public async Task<bool> ExecuteAsync(BootstrapContext context)
        {
            if (_modules.Count == 0)
            {
                Debug.LogWarning("[Bootstrap] No modules to initialize");
                return true;
            }

            // Resolve dependencies
            List<IBootstrapModule> sorted;
            try
            {
                sorted = TopologicalSort();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Bootstrap] Dependency resolution failed: {ex.Message}");
                return false;
            }

            // Log execution order
            if (_config.verboseLogging)
            {
                Debug.Log("[Bootstrap] Execution order:");
                for (int i = 0; i < sorted.Count; i++)
                {
                    var deps = sorted[i].Dependencies;
                    var depsStr = deps != null && deps.Length > 0 ? $" (deps: {string.Join(", ", deps)})" : "";
                    Debug.Log($"  {i + 1}. {sorted[i].ModuleName}{depsStr}");
                }
            }

            // Execute modules
            float progressStep = 1f / sorted.Count;
            for (int i = 0; i < sorted.Count; i++)
            {
                var module = sorted[i];
                
                // Notify module started
                float currentProgress = i * progressStep;
                context.OnModuleStarted?.Invoke(module.ModuleName, currentProgress);
                
                bool success = await ExecuteModule(module, context);

                if (!success)
                {
                    var provider = _config.modules.FirstOrDefault(p =>
                    {
                        var m = p.GetModule(null);
                        return m?.ModuleName == module.ModuleName;
                    });

                    if (provider?.isRequired == true)
                    {
                        Debug.LogError($"[Bootstrap] Required module {module.ModuleName} failed - aborting");
                        return false;
                    }
                    else
                    {
                        Debug.LogWarning($"[Bootstrap] Optional module {module.ModuleName} failed - continuing");
                    }
                }

                context.Progress?.Report((i + 1) * progressStep);
            }

            if (_config.verboseLogging)
                Debug.Log($"[Bootstrap] Complete in {context.Elapsed.TotalSeconds:F2}s");

            return true;
        }

        /// <summary>Execute a single module with retry.</summary>
        private async Task<bool> ExecuteModule(IBootstrapModule module, BootstrapContext context)
        {
            int maxAttempts = _config.retryOnFailure ? _config.maxRetries : 1;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (_config.verboseLogging)
                        Debug.Log($"[Bootstrap] Initializing {module.ModuleName}... ({attempt}/{maxAttempts})");

                    bool success = await module.InitializeAsync(context);

                    if (success)
                    {
                        if (_config.verboseLogging)
                            Debug.Log($"[Bootstrap] ✓ {module.ModuleName}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Bootstrap] Exception in {module.ModuleName}: {ex.Message}");
                }

                if (attempt < maxAttempts)
                {
                    Debug.LogWarning($"[Bootstrap] Retrying {module.ModuleName}...");
                    await Task.Delay(500);
                }
            }

            return false;
        }

        /// <summary>Topological sort with priority.</summary>
        private List<IBootstrapModule> TopologicalSort()
        {
            var sorted = new List<IBootstrapModule>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            void Visit(IBootstrapModule module)
            {
                if (visited.Contains(module.ModuleName))
                    return;

                if (visiting.Contains(module.ModuleName))
                    throw new InvalidOperationException($"Circular dependency: {module.ModuleName}");

                visiting.Add(module.ModuleName);

                // Visit dependencies first
                var deps = module.Dependencies;
                if (deps != null)
                {
                    foreach (var depName in deps)
                    {
                        if (!_moduleMap.TryGetValue(depName, out var dep))
                            throw new InvalidOperationException($"{module.ModuleName} depends on missing module: {depName}");
                        Visit(dep);
                    }
                }

                visiting.Remove(module.ModuleName);
                visited.Add(module.ModuleName);
                sorted.Add(module);
            }

            // Sort by priority, then apply topological sort
            var byPriority = _modules.OrderBy(m => m.Priority).ToList();
            foreach (var module in byPriority)
                Visit(module);

            return sorted;
        }
    }
}
