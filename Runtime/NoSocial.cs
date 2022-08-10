using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

namespace JTuresson.Social
{
    public class NoSocial : ISocialService
    {
        public byte[] CloudData => new byte[0];

        public bool UserCanSign => false;

        public bool CloudSaveEnabled => false;

        public string UserName => string.Empty;

        public string StoreName => string.Empty;

        public string Greeting => string.Empty;

        public bool AchievementsEnabled => false;

        public bool LeaderboardsEnabled => false;

        public bool Authenticated => false;

        public RuntimePlatform Platform => RuntimePlatform.WindowsEditor;

        public string LocalUserId => string.Empty;

        public event Action<bool> IsAuthenticatedChanged;

        public void IncrementAchievement(string achievementId, double steps, double maxSteps,
            Action<bool> callback)
        {
        }

        public void Initialize()
        {
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
        }

        public void LoadFromCloud(Action<bool> callback)
        {
        }

        public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
        {
        }

        public void Authenticate(Action<bool> callback)
        {
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback)
        {
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, string tag,
            Action<bool> callback)
        {
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
        }

        public void ShowAchievementsUI()
        {
        }

        public IAchievement CreateAchievement()
        {
            return new Achievement();
        }

        public void ShowLeaderboardUI()
        {
        }

        public ILeaderboard CreateLeaderboard()
        {
            return new Leaderboard();
        }

        public void ShowLeaderboardUI(string leaderboardId)
        {
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
        }
    }
}