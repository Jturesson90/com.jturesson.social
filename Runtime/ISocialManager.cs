using System;
using UnityEngine;

namespace JTuresson.Social
{
	public interface ISocialManager
	{
		bool Authenticated { get; }
		bool CloudSaveEnabled { get; }
		bool AchievementsEnabled { get; }
		bool LeaderboardsEnabled { get; }
		IAchievements Achievements { get; }
		ILeaderboards Leaderboards { get; }
		bool AuthenticatingPending { get; }

		RuntimePlatform Platform { get; }
		bool UserCanSign { get; }
		string StoreName { get; }
		string UserName { get; }
		event EventHandler<SocialManagerArgs> AuthenticatedChanged;
		event EventHandler<bool> AuthenticatingPendingChanged;
		event EventHandler<bool> OnUploadChanged;
		event EventHandler<OnUploadCompleteArgs> OnUploadComplete;
		event EventHandler<OnLoadFromCloudCompleteArgs> OnLoadFromCloudComplete;
		void Authenticate();
		void SaveGame(bool b);
	}
}