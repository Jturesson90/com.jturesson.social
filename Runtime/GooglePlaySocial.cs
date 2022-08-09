#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using JTuresson.Social.ScriptableObjects;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public class GooglePlaySocial : ISocialService
    {
        public bool Authenticated => _social.IsAuthenticated();
        public RuntimePlatform Platform => RuntimePlatform.Android;
        public event Action<bool> IsAuthenticatedChanged;
        public bool UserCanSign => true;
        public string UserName => _social.GetUserDisplayName();

        public bool IsLoggedIn => _social.localUser.authenticated;

        public string Name => _social.localUser.userName;
        public string StoreName { get; private set; }

        public byte[] CloudData { get; private set; }

        private readonly string cloudFileName;

        public bool CloudSaveEnabled { get; private set; }
        public bool AchievementsEnabled { get; private set; }
        public bool LeaderboardsEnabled { get; private set; }

        public string LocalUserId => _social.localUser.id;

        private readonly bool debugMode = false;
        private PlayGamesPlatform _social;

        public GooglePlaySocial(SocialAndroidSettingsSO settings, PlayGamesPlatform social)
        {
            LeaderboardsEnabled = settings.leaderboards;
            AchievementsEnabled = settings.achievements;
            CloudSaveEnabled = settings.cloudSave;
            cloudFileName = settings.cloudFileName;
            StoreName = settings.storeName;
            debugMode = settings.debugLog;
            _social = social ?? PlayGamesPlatform.Activate();
        }

        public void Initialize()
        {
        }

        public void LoadFromCloud(Action<bool> callback)
        {
            if (currentlyLoadingFromCloud || !IsLoggedIn)
            {
                Debug.LogWarning("GooglePlaySocial loading or is not LoggedIn");
                LoadComplete(false);
                return;
            }

            currentlyLoadingFromCloud = true;
            loadCallback = callback;

            _social.SavedGame.OpenWithAutomaticConflictResolution(cloudFileName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                SavedGameOpened);
        }


        public void Login(Action<bool> callback)
        {
            if (IsLoggedIn)
            {
                callback?.Invoke(false);
                return;
            }

            _social.Authenticate(status =>
            {
                callback?.Invoke(status == SignInStatus.Success);
                IsAuthenticatedChanged?.Invoke(IsLoggedIn);
            });
        }


        public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
        {
            if (savingToCloud)
            {
                SaveComplete(false);
            }

            ;
            saveCallback = callback;
            savingToCloud = true;

            _social.SavedGame.OpenWithAutomaticConflictResolution(cloudFileName,
                DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime,
                (SavedGameRequestStatus status, ISavedGameMetadata game) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        Debug.LogWarning("GooglePlaySocial OpenWithAutomaticConflictResolution Failed");
                        SaveComplete(false);
                    }

                    SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedPlayedTime(playedTime)
                        .WithUpdatedDescription("Saved Game at " + DateTime.Now);

                    SavedGameMetadataUpdate updatedMetadata = builder.Build();

                    _social.SavedGame
                        .CommitUpdate(game, updatedMetadata, data,
                            (SavedGameRequestStatus committedStatus, ISavedGameMetadata committedGame) =>
                            {
                                Debug.LogWarning("GooglePlaySocial SavedGameRequestStatus " +
                                                 committedStatus.ToString() + " " +
                                                 committedGame.Description + " committedGame: " +
                                                 committedGame.Filename);
                                SaveComplete(committedStatus == SavedGameRequestStatus.Success);
                            }
                        );
                }
            );
        }

        Action<bool> saveCallback;
        bool savingToCloud = false;

        private void SaveComplete(bool success)
        {
            savingToCloud = false;
            if (saveCallback != null)
            {
                saveCallback.Invoke(success);
            }

            saveCallback = null;
        }

        Action<bool> loadCallback;
        bool currentlyLoadingFromCloud = false;


        private void LoadComplete(bool success)
        {
            currentlyLoadingFromCloud = false;
            if (loadCallback != null)
            {
                loadCallback.Invoke(success);
            }

            loadCallback = null;
        }


        private void SavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                _social.SavedGame.ReadBinaryData(game, SavedGameLoaded);
            }
            else
            {
                LoadComplete(false);
            }
        }

        private void SavedGameLoaded(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                CloudData = data;
                LoadComplete(true);
            }
            else
            {
                LoadComplete(false);
            }
        }

        public void UnlockAchievement(string achievementId, Action<bool> callback)
        {
            _social.UnlockAchievement(achievementId, callback);
        }

        public void IncrementAchievement(string achievementId, double steps, double stepsRatio,
            Action<bool> callback)
        {
            _social.IncrementAchievement(achievementId, (int) steps, callback);
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            _social.LoadAchievements(callback);
        }

        public void ShowAchievementsUI()
        {
            _social.ShowAchievementsUI();
        }

        public IAchievement CreateAchievement()
        {
            return _social.CreateAchievement();
        }

        public void ShowLeaderboardUI()
        {
            _social.ShowLeaderboardUI();
        }

        public ILeaderboard CreateLeaderboard()
        {
            return _social.CreateLeaderboard();
        }

        public void ShowLeaderboardUI(string leaderboardId)
        {
            _social.ShowLeaderboardUI(leaderboardId, (UIStatus status) =>
            {
                if (status < 0)
                {
                    Debug.LogError("ShowLeaderboardUI " + status);
                }
            });
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, Action<bool> callback)
        {
            _social.ReportScore(score, leaderboardId, callback);
        }

        public void ReportLeaderboardScore(long score, string leaderboardId, string tag,
            Action<bool> callback)
        {
            _social.ReportScore(score, leaderboardId, tag, callback);
        }

        public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
        {
            leaderboard.SetUserFilter(new string[] {_social.localUser.id});
            leaderboard.LoadScores(callback);
        }
    }
}
#endif