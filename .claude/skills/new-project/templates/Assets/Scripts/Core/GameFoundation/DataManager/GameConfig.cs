using System;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    [Serializable]
    public class GameConfig
    {
        public string bundleVersion = "1.0.0";
        public int forceVersion;
        public bool inAppUpdate;
        public bool FTUE_Play = true;
        public bool FTUE_Data = true;
    }
}
