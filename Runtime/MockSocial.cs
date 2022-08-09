using System;
using System.Threading.Tasks;
using JTuresson.Social.IO;
using JTuresson.Social.ScriptableObjects;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public class MockSocial : ISocialService
    {
        public RuntimePlatform Platform => RuntimePlatform.WindowsEditor;

        public bool UserCanSign => true;

        private bool _isLoggedIn;

        public bool Authenticated
        {
            get => _isLoggedIn;
            private set
            {
                if (_isLoggedIn == value) return;
                _isLoggedIn = value;
                IsAuthenticatedChanged?.Invoke(value);
            }
        }

        public event Action<bool> IsAuthenticatedChanged;
        public string UserName => Authenticated ? LocalUserId : string.Empty;
        public string StoreName { get; private set; }

        public byte[] CloudData { get; private set; }

        public bool CloudSaveEnabled { get; private set; }
        public bool AchievementsEnabled { get; private set; }

        public bool LeaderboardsEnabled { get; private set; }

        public string LocalUserId { get; }

        private readonly string _cloudFileName;

        private readonly IFileManager _fileManager;

        private readonly ISocialPlatform _social;

        public MockSocial(SocialMockSettingsSO settings, IFileManager fileManager,
            ISocialPlatform social)
        {
            LeaderboardsEnabled = settings.leaderboards;
            AchievementsEnabled = settings.achievements;
            CloudSaveEnabled = settings.cloudSave;
            _cloudFileName = settings.cloudFileName;
            LocalUserId = settings.userName;
            StoreName = settings.storeName;
            _fileManager = fileManager;
            _social = social;
        }

        public void Initialize()
        {
            Authenticated = false;
        }

        public void Login(Action<bool> callback)
        {
            if (Authenticated)
            {
                callback?.Invoke(false);
                return;
            }

            _social.localUser.Authenticate((bool success) => { });
            callback?.Invoke(Authenticated = true);
            IsAuthenticatedChanged?.Invoke(Authenticated);
        }

        public void Logout(Action<bool> callback)
        {
            Authenticated = false;
            callback?.Invoke(true);
        }

        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            bool success = false;
            if (!CloudSaveEnabled)
            {
                callback?.Invoke(success);
                return;
            }

            CloudData = data;
            try
            {
                _fileManager.WriteToFile($"{_cloudFileName}.txt",
                    System.Text.ASCIIEncoding.Default.GetString(data));
                success = true;
            }
            catch
            {
                success = false;
            }

            callback?.Invoke(success);
            // TODO FIX to call on mnain thread // UseDelay(1.5f, () => callback?.Invoke(success));
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            var success = false;
            if (!CloudSaveEnabled)
            {
                callback?.Invoke(false);
                return;
            }

            try
            {
                _fileManager.LoadFromFile($"{_cloudFileName}.txt", out var json);
                CloudData = System.Text.ASCIIEncoding.Default.GetBytes(json);
                success = true;
            }
            catch
            {
                success = false;
            }

            callback?.Invoke(success);
            // UseDelay(loginDelay, () => callback?.Invoke(success));
        }

        // TODO Fix, call back on main thread
        void UseDelay(float time, Action callback)
        {
            Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(time));
                callback?.Invoke();
            });
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            callback(true);
        }

        public void IncrementAchievement(string achievementId, double steps, double stepsToComplete,
            Action<bool> callback)
        {
            callback(true);
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            callback(new IAchievement[0]);
        }

        public void ShowLeaderboardUI()
        {
            _social.ShowLeaderboardUI();
        }

        public ILeaderboard CreateLeaderboard()
        {
            throw new NotImplementedException();
        }

        public void ShowLeaderboardUI(string leaderboardId)
        {
            ShowLeaderboardUI();
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback)
        {
            Debug.LogWarning("ReportLeaderboardScore not implemented in Editor");
            callback?.Invoke(false);
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, string tag,
            Action<bool> callback)
        {
            ReportLeaderboardScore(score, leaderboardId, callback);
        }

        public void ShowAchievementsUI()
        {
            _social.ShowLeaderboardUI();
        }

        public IAchievement CreateAchievement()
        {
            throw new NotImplementedException();
        }

        public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
        {
            Debug.LogWarning("LoadUserLeaderboardScore not implemented in Editor");
            callback?.Invoke(false);
        }
    }
}