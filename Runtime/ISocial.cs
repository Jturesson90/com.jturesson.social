using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public interface ISocialService : ISocialAchievements, ISocialLeaderboards, ISession, ICloudSave
    {
        void Initialize();
        void Login(Action<bool> callback);
        bool UserCanSign { get; }
        string UserName { get; }
        string StoreName { get; }
    }

    public interface ICloudSave
    {
        bool CloudSaveEnabled { get; }
        void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback);
        void LoadFromCloud(Action<bool> callback);
        byte[] CloudData { get; }
    }

    public interface ISocialAchievements
    {
        bool AchievementsEnabled { get; }
        void LoadAchievements(Action<IAchievement[]> callback);
        void UnlockAchievement(string achievementId, Action<bool> callback);

        void IncrementAchievement(string achievementId, double steps, double maxSteps,
            Action<bool> callback);

        void ShowAchievementsUI();
        IAchievement CreateAchievement();
    }

    public interface ISession
    {
        bool Authenticated { get; }
        RuntimePlatform Platform { get; }
        event Action<bool> IsAuthenticatedChanged;
        string LocalUserId { get; }
    }

    public interface ISocialLeaderboards
    {
        bool LeaderboardsEnabled { get; }
        void ShowLeaderboardUI();
        ILeaderboard CreateLeaderboard();
        void ShowLeaderboardUI(string leaderboardId);
        void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback);
        void ReportLeaderboardScore(long score, string leaderboardId, string tag, Action<bool> callback);
        void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback);
    }
}