using System;
using JTuresson.Social.IO;
using JTuresson.Social.ScriptableObjects;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public class SocialManager : MonoBehaviour, ISocialManager
    {
        [Header("Platform settings")] [SerializeField]
        private SocialAndroidSettingsSO androidSettings = default;

        [SerializeField] private SocialIOSSettingsSO iosSettings = default;
        [SerializeField] private SocialMockSettingsSO mockSettings = default;

        private ISocialService _socialService;
        public event EventHandler<SocialManagerArgs> LoggedInChanged;
        public event EventHandler<bool> LoggingInPendingChanged;
        public event EventHandler<bool> OnUploadChanged;
        public event EventHandler<OnUploadCompleteArgs> OnUploadComplete;
        public event EventHandler<OnLoadFromCloudCompleteArgs> OnLoadFromCloudComplete;

        private bool _loggingInPending = false;

        public bool LoggingInPending
        {
            get => _loggingInPending;
            private set
            {
                if (_loggingInPending != value)
                {
                    _loggingInPending = value;
                    LoggingInPendingChanged?.Invoke(this, _loggingInPending);
                }
            }
        }

        private bool _uploadPending = false;

        public bool UploadPending
        {
            get => _uploadPending;
            private set
            {
                if (_uploadPending != value)
                {
                    _uploadPending = value;
                    OnUploadChanged?.Invoke(this, _uploadPending);
                }
            }
        }

        public byte[] CloudData => _socialService.CloudData;
        public string UserName => _socialService.UserName;
        public bool IsLoggedIn => _socialService.Authenticated;
        public bool UserCanSign => _socialService.UserCanSign;
        public string StoreName => _socialService.StoreName;
        public bool CloudSaveEnabled => _socialService.CloudSaveEnabled;
        public RuntimePlatform Platform => _socialService.Platform;
        public bool SocialEnabled { get; private set; }

        private CloudSaveData _cloudSaveData;
        public IAchievements Achievements { get; private set; }
        private Achievements _achievements;

        public ILeaderboards Leaderboards { get; private set; }
        private Leaderboards _leaderboards;

        protected void Awake()
        {
#if UNITY_EDITOR
            if (mockSettings != null)
            {
                _socialService = new MockSocial(mockSettings, new FileManager(),
                    new Local());
            }
#elif UNITY_ANDROID
            if (androidSettings != null)
            {
                _socialService = new GooglePlaySocial(androidSettings, null);
            }
#elif UNITY_IOS
            if (iosSettings != null)
            {
                _socialService = new IOSSocial(iosSettings, UnityEngine.Social.Active);
            }
#endif

            if (_socialService == null)
            {
                _socialService = new NoSocial();
                SocialEnabled = false;
            }
            else
            {
                SocialEnabled = true;
            }

            _socialService.Initialize();

            _achievements = new Achievements(_socialService, _socialService);
            Achievements = _achievements;

            _leaderboards = new Leaderboards(_socialService, _socialService);
            Leaderboards = _leaderboards;
        }

        public void SetSaveDataBase(CloudSaveData saveData)
        {
            _cloudSaveData = saveData;
        }

        private void Start()
        {
            if (!SocialEnabled) return;

            Login();
            if (_socialService.AchievementsEnabled)
            {
                _achievements.Initialize();
            }

            if (_socialService.LeaderboardsEnabled)
            {
                _leaderboards.Initialize();
            }
        }

        protected void OnDestroy()
        {
            if (SocialEnabled)
            {
                if (_socialService.AchievementsEnabled)
                {
                    _achievements.Save();
                }
            }
        }

        public void Login()
        {
            if (!SocialEnabled)
            {
                throw new InvalidOperationException(
                    "Must enable Social before attempting to login");
            }

            LoggingInPending = true;
            _socialService.Login((bool success) =>
                {
                    if (success && CloudSaveEnabled)
                        LoadFromCloud();

                    LoggingInPending = false;
                    LoggedInChanged?.Invoke(this, new SocialManagerArgs()
                    {
                        IsLoggedIn = _socialService.Authenticated,
                        Platform = _socialService.Platform,
                        Name = _socialService.UserName
                    });
                }
            );
        }

        public void SaveGame(bool manual = false)
        {
            if (!SocialEnabled || !CloudSaveEnabled || _cloudSaveData == null)
            {
                throw new InvalidOperationException(
                    "Must enable Social, Cloud and save data cant be null");
            }

            UploadPending = true;
            TimeSpan timePlayed;
            try
            {
                timePlayed = TimeSpan.FromSeconds(_cloudSaveData.totalPlayingTime);
            }
            catch
            {
                timePlayed = TimeSpan.FromSeconds(3600);
            }

            _socialService.SaveGame(_cloudSaveData.ToBytes(), timePlayed, (bool success) =>
            {
                UploadPending = false;
                if (success)
                {
                    OnUploadComplete?.Invoke(this, new OnUploadCompleteArgs() {Manual = manual});
                }
                else
                {
                    Debug.LogWarning("SocialManager SaveGame FAIL");
                }
            });
        }

        public void LoadFromCloud()
        {
            if (!SocialEnabled || !CloudSaveEnabled)
            {
                throw new InvalidOperationException("Must enable Social and Cloud save before");
            }

            Debug.LogWarning("SocialManager LoadFromCloud");
            _socialService.LoadFromCloud((bool success) =>
            {
                Debug.LogWarning("SocialManager LoadFromCloud success? " + success);
                OnLoadFromCloudComplete?.Invoke(this,
                    new OnLoadFromCloudCompleteArgs()
                    {
                        Data = success ? CloudSaveData.FromBytes(CloudData) : null,
                        Success = success
                    });
            });
        }
    }

    public class SocialManagerArgs : EventArgs
    {
        public bool IsLoggedIn { get; set; }
        public RuntimePlatform Platform { get; set; }
        public string Name { get; internal set; }
    }

    public class OnUploadCompleteArgs : EventArgs
    {
        public bool Manual { get; set; }
    }

    public class OnLoadFromCloudCompleteArgs : EventArgs
    {
        public bool Success { get; set; }
        public CloudSaveData Data { get; set; }
    }
}