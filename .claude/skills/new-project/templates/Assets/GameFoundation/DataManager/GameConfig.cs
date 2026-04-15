using System;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    [Serializable]
    public class GameConfig
{
    public string bundleVersion = "1.0.0";
    public string forceVersion = "0.0.0";
    public bool inAppUpdate = true;
    public bool FTUE_Play = true;
    public string FTUE_Data = "";

    // ADS Time Config
    public float timePlayToShowAd = 15f;
    public float timePlayToShowAdReduce = 15f;
    public float timeToWaitOpenAd = 8.6f;
    public float timePlayToShowOpenAd = 8.6f;
    public int adShowFromLevel;
    public bool adInterOnStart = true;
    public bool adInterOnComplete = true;
    public int adShowInterFTUE = 1;
    public bool adUseBackup;
    public bool adUseBackupRewardInter;
    public bool adUseBackupBannerInter;
    public bool adUseOpenBackup;
    public bool adUseNative;
    public bool adUseNativeClickReLoad = true;
    public float adUseNativeTimeReLoad = 15f;
    public bool adUseInPlay;
    public bool forceInterToReward;
    public int rewardLevel = 50;
    public int rewardByAd = 500;
    public float rewardLevelScale = 0.05f;
    public int reviceCountDown = 5;
    public int reviceCountMax = 3;
    public int reviceNoThankCountDown = 2;
}
}
