using System;
using UnityEngine;

namespace JTuresson.Social
{
    public interface ISocialManager
    {
        public bool Authenticated { get; }
        public bool CloudSaveEnabled { get; }
        public bool AchievementsEnabled { get; }
        public bool LeaderboardsEnabled { get; }
        public IAchievements Achievements { get; }
        public ILeaderboards Leaderboards { get; }

        public RuntimePlatform Platform { get; }
        public event EventHandler<SocialManagerArgs> AuthenticatedChanged;
        public event EventHandler<bool> AuthenticatingPendingChanged;
        public event EventHandler<bool> OnUploadChanged;
        public event EventHandler<OnUploadCompleteArgs> OnUploadComplete;
        public event EventHandler<OnLoadFromCloudCompleteArgs> OnLoadFromCloudComplete;
    }
}