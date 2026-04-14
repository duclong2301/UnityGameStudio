using System;
using UnityEngine;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    [Serializable]
    public class CollectionDataSave
    {
        public string key;
        public SaveData data;
    }

    [Serializable]
    public class DataCollectionEntry
    {
        [Tooltip("Unique key matching ISavableCollection.CollectionKey")]
        public string key;

        [Tooltip("ScriptableObject implementing ISavableCollection")]
        public ScriptableObject source;

        [NonSerialized] public ScriptableObject runtimeInstance;

        public ISavableCollection Savable => runtimeInstance as ISavableCollection;
    }
}
