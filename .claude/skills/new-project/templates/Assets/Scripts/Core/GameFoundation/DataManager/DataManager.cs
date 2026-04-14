using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    /// <summary>
    /// Singleton persistence manager. Loads/Saves <see cref="GameData"/> to a binary file.
    /// Extensible via <see cref="DataCollectionEntry"/> list in the Inspector — add a collection
    /// without modifying this class.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        [SerializeField] private string fileName = "gamedata.bin";
        [SerializeField] private List<DataCollectionEntry> collections = new List<DataCollectionEntry>();

        public GameData Data { get; private set; } = new GameData();
        private string FilePath => Path.Combine(Application.persistentDataPath, fileName);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var entry in collections)
            {
                if (entry.source == null) continue;
                entry.runtimeInstance = Instantiate(entry.source);
            }

            Load();
        }

        public void Save()
        {
            foreach (var entry in collections)
            {
                var savable = entry.Savable;
                if (savable == null) continue;
                Data.SetCollection(entry.key, savable.ExportSaveData());
            }

            try
            {
                using var stream = File.Create(FilePath);
                new BinaryFormatter().Serialize(stream, Data);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DataManager] Save failed: {ex.Message}");
            }
        }

        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                Data = new GameData();
                ApplyToCollections();
                return;
            }

            try
            {
                using var stream = File.OpenRead(FilePath);
                Data = (GameData)new BinaryFormatter().Deserialize(stream);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DataManager] Load failed, resetting. {ex.Message}");
                Data = new GameData();
            }

            ApplyToCollections();
        }

        private void ApplyToCollections()
        {
            foreach (var entry in collections)
            {
                var savable = entry.Savable;
                if (savable == null) continue;
                var saved = Data.GetCollection(entry.key);
                if (saved?.data != null) savable.ImportSaveData(saved.data);
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();
    }
}
