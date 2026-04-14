namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    public interface ISavableCollection
    {
        string CollectionKey { get; }
        SaveData ExportSaveData();
        void ImportSaveData(SaveData data);
        void ResetData();
    }
}
