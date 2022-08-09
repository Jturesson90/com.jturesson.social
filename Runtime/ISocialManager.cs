using System;
using UnityEngine;

namespace JTuresson.Social
{
    public interface ISocialManager
    {
        public bool IsLoggedIn { get; }
        public bool CloudSaveEnabled { get; }
        public IAchievements Achievements { get; }
        public ILeaderboards Leaderboards { get; }

        public RuntimePlatform Platform { get; }
        public event EventHandler<SocialManagerArgs> LoggedInChanged;
        public event EventHandler<bool> LoggingInPendingChanged;
        public event EventHandler<bool> OnUploadChanged;
        public event EventHandler<OnUploadCompleteArgs> OnUploadComplete;
        public event EventHandler<OnLoadFromCloudCompleteArgs> OnLoadFromCloudComplete;
    }
}