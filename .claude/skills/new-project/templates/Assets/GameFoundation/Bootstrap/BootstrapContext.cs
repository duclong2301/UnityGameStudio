using System;
using System.Collections.Generic;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap
{
    /// <summary>
    /// Shared context passed to all modules during initialization.
    /// Provides service registry and data sharing.
    /// </summary>
    public class BootstrapContext
    {
        // Service registry
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<string, object> _namedServices = new Dictionary<string, object>();

        // Shared data
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        // Progress tracking
        public IProgress<float> Progress { get; }
        public DateTime StartTime { get; }

        /// <summary>
        /// Callback invoked when a module starts initializing.
        /// Parameters: (moduleName, currentProgress)
        /// </summary>
        public Action<string, float> OnModuleStarted { get; set; }

        public BootstrapContext(IProgress<float> progress = null)
        {
            Progress = progress;
            StartTime = DateTime.Now;
        }

        // ──────────────────────────────────────────────────────────────
        // Service Registration
        // ──────────────────────────────────────────────────────────────

        /// <summary>Register a service by type.</summary>
        public void RegisterService<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        /// <summary>Register a service with a custom name.</summary>
        public void RegisterService<T>(string name, T service) where T : class
        {
            _services[typeof(T)] = service;
            _namedServices[name] = service;
        }

        /// <summary>Get a service by type. Throws if not found.</summary>
        public T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return service as T;
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        }

        /// <summary>Try get a service by type. Returns null if not found.</summary>
        public T TryGetService<T>() where T : class
        {
            return _services.TryGetValue(typeof(T), out var service) ? service as T : null;
        }

        /// <summary>Get a service by name.</summary>
        public T GetServiceByName<T>(string name) where T : class
        {
            if (_namedServices.TryGetValue(name, out var service))
                return service as T;
            throw new InvalidOperationException($"Service '{name}' not registered");
        }

        // ──────────────────────────────────────────────────────────────
        // Shared Data
        // ──────────────────────────────────────────────────────────────

        /// <summary>Store shared data for other modules.</summary>
        public void SetData(string key, object value)
        {
            _data[key] = value;
        }

        /// <summary>Get shared data. Returns null if not found.</summary>
        public T GetData<T>(string key) where T : class
        {
            return _data.TryGetValue(key, out var value) ? value as T : null;
        }

        /// <summary>Check if data exists.</summary>
        public bool HasData(string key) => _data.ContainsKey(key);

        // ──────────────────────────────────────────────────────────────
        // Utilities
        // ──────────────────────────────────────────────────────────────

        /// <summary>Elapsed time since initialization started.</summary>
        public TimeSpan Elapsed => DateTime.Now - StartTime;
    }
}
