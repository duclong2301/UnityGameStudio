using System;
using System.Collections.Generic;

namespace {{PROJECT_NAMESPACE}}.GameFoundation.Data
{
    [Serializable]
    public class GameData
{
    public GameConfig gameConfig = new GameConfig();
    public UserData userData = new UserData { name = "YOU" };
    public List<CollectionDataSave> collections = new List<CollectionDataSave>();

    /// <summary>
    /// Returns a shallow clone of core data with an empty collections list.
    /// Used as the default baseline before a save file is loaded.
    /// </summary>
    public GameData Clone() => new GameData
    {
        gameConfig = new GameConfig
        {
            bundleVersion = gameConfig.bundleVersion,
            forceVersion = gameConfig.forceVersion,
            inAppUpdate = gameConfig.inAppUpdate,
            FTUE_Play = gameConfig.FTUE_Play,
            FTUE_Data = gameConfig.FTUE_Data,
            timePlayToShowAd = gameConfig.timePlayToShowAd,
            timePlayToShowAdReduce = gameConfig.timePlayToShowAdReduce,
            timeToWaitOpenAd = gameConfig.timeToWaitOpenAd,
            timePlayToShowOpenAd = gameConfig.timePlayToShowOpenAd,
            adShowFromLevel = gameConfig.adShowFromLevel,
            adInterOnStart = gameConfig.adInterOnStart,
            adInterOnComplete = gameConfig.adInterOnComplete,
            adShowInterFTUE = gameConfig.adShowInterFTUE,
            adUseBackup = gameConfig.adUseBackup,
            adUseBackupRewardInter = gameConfig.adUseBackupRewardInter,
            adUseBackupBannerInter = gameConfig.adUseBackupBannerInter,
            adUseOpenBackup = gameConfig.adUseOpenBackup,
            adUseNative = gameConfig.adUseNative,
            adUseNativeClickReLoad = gameConfig.adUseNativeClickReLoad,
            adUseNativeTimeReLoad = gameConfig.adUseNativeTimeReLoad,
            adUseInPlay = gameConfig.adUseInPlay,
            forceInterToReward = gameConfig.forceInterToReward,
            rewardLevel = gameConfig.rewardLevel,
            rewardByAd = gameConfig.rewardByAd,
            rewardLevelScale = gameConfig.rewardLevelScale,
            reviceCountDown = gameConfig.reviceCountDown,
            reviceCountMax = gameConfig.reviceCountMax,
            reviceNoThankCountDown = gameConfig.reviceNoThankCountDown,
        },
        userData = new UserData
        {
            name = userData.name,
            totalScore = userData.totalScore,
            playCount = userData.playCount,
            lastPlayedTimestamp = userData.lastPlayedTimestamp,
        },
        // collections intentionally empty — loaded from save file or populated at runtime
        collections = new List<CollectionDataSave>(),
    };

    public CollectionDataSave GetCollection(string key)
        => collections.Find(c => c.key == key);

    public void SetCollection(string key, SaveData data)
    {
        var existing = collections.Find(c => c.key == key);
        if (existing != null)
            existing.data = data;
        else
            collections.Add(new CollectionDataSave { key = key, data = data });
    }
}
}
