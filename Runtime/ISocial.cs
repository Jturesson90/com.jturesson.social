using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
	public interface ISocialService : ISocialAchievements, ISocialLeaderboards, ISession, ICloudSave
	{
		bool UserCanSign { get; }
		string UserName { get; }
		string StoreName { get; }
		void Initialize();
		void Authenticate(Action<bool> callback);
	}

	public interface ICloudSave
	{
		bool CloudSaveEnabled { get; }
		byte[] CloudData { get; }
		void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback);
		void LoadFromCloud(Action<bool> callback);
	}

	public interface ISocialAchievements
	{
		bool AchievementsEnabled { get; }
		void LoadAchievements(Action<IAchievement[]> callback);
		void UnlockAchievement(string achievementId, Action<bool> callback);

		void IncrementAchievement(string achievementId, double steps, double maxSteps,
			Action<bool> callback);

		void ShowAchievementsUI();
	}

	public interface ISession
	{
		bool Authenticated { get; }
		RuntimePlatform Platform { get; }
		string LocalUserId { get; }
		event Action<bool> IsAuthenticatedChanged;
	}

	public interface ISocialLeaderboards
	{
		bool LeaderboardsEnabled { get; }
		void ShowLeaderboardUI();
		ILeaderboard CreateLeaderboard();
		void ShowLeaderboardUI(string leaderboardId);
		void ReportLeaderboardTime(long milliseconds, string leaderboardId, string tag, Action<bool> callback);
		void ReportLeaderboardInteger(int score, string leaderboardId, Action<bool> callback);
		void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback);
	}
}