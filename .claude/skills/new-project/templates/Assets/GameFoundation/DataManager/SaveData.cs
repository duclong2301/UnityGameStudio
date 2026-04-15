using System;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    /// <summary>
    /// Abstract base for all collection save payloads.
    /// BinaryFormatter preserves concrete type info — no [SerializeReference] needed.
    /// </summary>
    [Serializable]
    public abstract class SaveData { }
}
