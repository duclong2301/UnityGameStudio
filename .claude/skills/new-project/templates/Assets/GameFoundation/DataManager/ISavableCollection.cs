namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    /// <summary>
    /// Interface for ScriptableObject collections that can be saved/loaded via DataManager.
    /// Each collection defines a concrete XxxSave : SaveData and implements these methods.
    /// BinaryFormatter preserves type info — no [SerializeReference] needed.
    /// </summary>
    public interface ISavableCollection
{
    string CollectionKey { get; }
    SaveData ExportSaveData();
    void ImportSaveData(SaveData data);
    void UnlockAll();
    void ResetData();
}
}
