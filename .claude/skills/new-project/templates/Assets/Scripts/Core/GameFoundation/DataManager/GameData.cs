using System;
using System.Collections.Generic;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    [Serializable]
    public class GameData
    {
        public GameConfig gameConfig = new GameConfig();
        public UserData userData = new UserData();
        public List<CollectionDataSave> collections = new List<CollectionDataSave>();

        public CollectionDataSave GetCollection(string key)
            => collections.Find(c => c.key == key);

        public void SetCollection(string key, SaveData data)
        {
            var existing = collections.Find(c => c.key == key);
            if (existing != null) existing.data = data;
            else collections.Add(new CollectionDataSave { key = key, data = data });
        }
    }
}
