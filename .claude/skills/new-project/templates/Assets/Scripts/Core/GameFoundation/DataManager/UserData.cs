using System;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    [Serializable]
    public class UserData
    {
        public string name = "YOU";
        public int totalScore;
        public int playCount;
        public long lastPlayedTimestamp;
    }
}
