using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using {{PROJECT_NAMESPACE}}.GameFoundation.Bootstrap;
using System.Threading.Tasks;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    /// <summary>
    /// Singleton persistence manager. Serializes <see cref="GameData"/> to a binary file via BinaryFormatter.
    /// Collections are registered in the Inspector via <see cref="DataCollectionEntry"/> list —
    /// add/remove collections without modifying this class.
    /// </summary>
    public class DataManager : MonoBehaviour, IBootstrapModule
    {
        #region bootstrap
        public string ModuleName => "DataManager";

        public int Priority => 10; // Early initialization

        public string[] Dependencies => null; // No dependencies
        #endregion

        protected static DataManager instance;
        public static bool IsLoaded = false;

        public delegate void LoadedDelegate();
        public static event LoadedDelegate OnLoaded;

        [SerializeField] protected UnityEvent onLoadCompleted;
        [SerializeField] protected GameData gameData = new GameData();

        public static GameData GameData;
        public static GameConfig GameConfig;
        public static UserData UserData;


        [Header("Data Collections")]
        [SerializeField]
        protected List<DataCollectionEntry> dataCollections = new List<DataCollectionEntry>();

        [Space]
        [SerializeField] private bool autoLoadOnStart = false;

        private static string SavePath => Path.Combine(Application.persistentDataPath, "savedata.bin");


        float loadProgress = 0f;
        string meessage = "Loading...";

        // ─── Lifecycle ────────────────────────────────────────────────────────────


        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Start()
        {
            if (autoLoadOnStart && !IsLoaded)
                StartCoroutine(IELoad());
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationQuit() => Save();

        // ─── Load ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Triggers a load from any static context. No-op if no instance exists.
        /// </summary>
        public static void Load()
        {
            if (instance != null)
                instance.StartCoroutine(instance.IELoad());
            else
                Debug.LogError("[DataManager] No instance found to load data.");
        }
        public async Task<bool> InitializeAsync(BootstrapContext context)
        {
            Debug.Log("[DataManager] Initialization started via bootstrap...");
            
            // Reset progress tracking
            loadProgress = 0f;
            meessage = "Initializing DataManager...";
            
            // Create a TaskCompletionSource to bridge coroutine → Task
            var tcs = new TaskCompletionSource<bool>();
            
            // Start the loading coroutine with completion callback
            StartCoroutine(IELoadWithCompletion(tcs));
            
            // Poll progress and report to bootstrap context
            string lastMessage = "";
            while (!tcs.Task.IsCompleted)
            {
                // Report progress to bootstrap pipeline
                context.Progress?.Report(loadProgress);
                
                // Log message changes
                if (meessage != lastMessage)
                {
                    Debug.Log($"[DataManager] {meessage}");
                    lastMessage = meessage;
                }
                
                await Task.Delay(100); // Check every 100ms
            }
            
            // Final progress report
            context.Progress?.Report(1f);
            Debug.Log("[DataManager] Loaded successfully");
            
            return await tcs.Task;
        }

        /// <summary>
        /// Wrapper coroutine that completes the TaskCompletionSource when IELoad finishes.
        /// </summary>
        private IEnumerator IELoadWithCompletion(TaskCompletionSource<bool> tcs)
        {
            yield return IELoad();
            tcs.SetResult(true);
        }

        protected IEnumerator IELoad()
        {
            if (IsLoaded)
            {
                OnLoaded?.Invoke();
                onLoadCompleted?.Invoke();
                yield break;
            }

            // Initialize defaults from the inspector-configured gameData asset
            GameData = gameData.Clone();
            GameConfig = GameData.gameConfig;
            UserData = GameData.userData;

            meessage = "Creating runtime instances of data collections...";
            // Instantiate runtime copies of all registered collections
            foreach (var entry in dataCollections)
            {
                if (entry.source != null)
                    entry.runtimeInstance = Instantiate(entry.source);
            }
            yield return new WaitForSeconds(0.5f);
            loadProgress = 0.5f;
            meessage = "Loading save data...";
            // Attempt to read persisted save file
            var saveData = ReadSave();
            if (saveData != null)
            {
                GameData = saveData;
                GameConfig = GameData.gameConfig;
                UserData = GameData.userData;

                foreach (var entry in dataCollections)
                {
                    if (entry.Savable == null) continue;
                    var saved = GameData.GetCollection(entry.key);
                    if (saved?.data != null)
                        entry.Savable.ImportSaveData(saved.data);
                }
            }

            OnAfterDataLoaded();
            yield return new WaitForSeconds(0.5f);
            meessage = "Loading all mostly done... finalizing.";
            loadProgress = 0.9f;
            float time = 3f; // Simulate loading time
            while (time > 0f)
            {
                time -= 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            meessage = "Loading Done!";
            loadProgress = 1f;
            yield return new WaitForSeconds(0.25f);
            IsLoaded = true;
            OnLoaded?.Invoke();
            onLoadCompleted?.Invoke();
            yield return null;
        }

        /// <summary>
        /// Override in subclasses to run logic immediately after data is loaded/reset,
        /// before <see cref="OnLoaded"/> fires.
        /// </summary>
        protected virtual void OnAfterDataLoaded() { }

        // ─── Save ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Flushes all collection data to disk. No-op if not yet loaded.
        /// </summary>
        public static void Save()
        {
            if (instance == null || !IsLoaded) return;

            GameData.gameConfig = GameConfig;
            GameData.userData = UserData;
            GameData.collections.Clear();

            foreach (var entry in instance.dataCollections)
            {
                if (entry.Savable != null)
                    GameData.SetCollection(entry.key, entry.Savable.ExportSaveData());
            }

            WriteSave(GameData);
        }

        // ─── Collection Access ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the runtime instance of a registered collection by key.
        /// </summary>
        public static T GetCollection<T>(string key) where T : ScriptableObject
        {
            if (instance == null) return null;
            var entry = instance.dataCollections.Find(e => e.key == key);
            return entry?.runtimeInstance as T;
        }

        // ─── Debug / Editor Utilities ─────────────────────────────────────────────

        /// <summary>
        /// Calls <see cref="ISavableCollection.UnlockAll"/> on every registered collection then saves.
        /// </summary>
        public void UnlockAll()
        {
            foreach (var entry in dataCollections)
                entry.Savable?.UnlockAll();
            Save();
        }

        /// <summary>
        /// Resets all collections to their default state, deletes the save file, and reloads.
        /// </summary>
        public void ResetData()
        {
            foreach (var entry in dataCollections)
            {
                entry.Savable?.ResetData();
#if UNITY_EDITOR
                if (entry.source != null)
                    UnityEditor.EditorUtility.SetDirty(entry.source);
#endif
            }

            if (File.Exists(SavePath))
                File.Delete(SavePath);

            IsLoaded = false;
            Debug.Log("[DataManager] ResetData — save file deleted. Reloading...");
            StartCoroutine(IELoad());
        }

        // ─── Binary I/O ───────────────────────────────────────────────────────────

        private static void WriteSave(GameData data)
        {
            using var stream = new FileStream(SavePath, FileMode.Create);
            new BinaryFormatter().Serialize(stream, data);
            Debug.Log($"[DataManager] Saved → {SavePath}");
        }

        private static GameData ReadSave()
        {
            if (!File.Exists(SavePath)) return null;
            try
            {
                using var stream = new FileStream(SavePath, FileMode.Open);
                return new BinaryFormatter().Deserialize(stream) as GameData;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataManager] Save file corrupted — starting fresh.\n{ex.Message}");
                return null;
            }
        }



    }
}

