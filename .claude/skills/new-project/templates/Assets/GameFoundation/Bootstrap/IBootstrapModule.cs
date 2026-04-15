using System.Threading.Tasks;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap
{
    /// <summary>
    /// Pure interface for bootstrap modules.
    /// Any class (MonoBehaviour, ScriptableObject, plain C#) can implement this.
    /// No inheritance required - maximum flexibility.
    /// </summary>
    public interface IBootstrapModule
    {
        /// <summary>
        /// Unique identifier for this module.
        /// Used for dependency resolution and logging.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Initialization priority (lower = earlier).
        /// Recommended: 0-100 range, step by 10.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Dependencies that must initialize before this module.
        /// Return empty array or null if no dependencies.
        /// </summary>
        string[] Dependencies { get; }

        /// <summary>
        /// Initialize this module.
        /// Return true on success, false on failure.
        /// </summary>
        /// <param name="context">Shared initialization context</param>
        Task<bool> InitializeAsync(BootstrapContext context);
    }
}
