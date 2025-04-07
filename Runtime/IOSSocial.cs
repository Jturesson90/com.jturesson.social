#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Collections.Generic;
using System.Linq;
using JTuresson.Social.ScriptableObjects;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

namespace JTuresson.Social
{
	public class IOSSocial : ISocialService
	{
		private readonly ISocialPlatform _socialPlatform;

		private Dictionary<string, IAchievement> achievementById;

		public IOSSocial(SocialSettingsSO settings, ISocialPlatform socialPlatform)
		{
			LeaderboardsEnabled = settings.leaderboards;
			AchievementsEnabled = settings.achievements;
			CloudSaveEnabled = settings.cloudSave;
			StoreName = settings.storeName;
			_socialPlatform = socialPlatform;
		}

		public RuntimePlatform Platform => RuntimePlatform.IPhonePlayer;

		public bool UserCanSign => false;

		public string StoreName { get; }

		public byte[] CloudData { get; }

		public bool CloudSaveEnabled { get; }

		public bool AchievementsEnabled { get; }

		public bool LeaderboardsEnabled { get; }


		public string LocalUserId => _socialPlatform.localUser.id;
		public bool Authenticated => _socialPlatform.localUser.authenticated;

		public string UserName => _socialPlatform.localUser.userName;

		public void Initialize()
		{
			GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
			achievementById = new Dictionary<string, IAchievement>();
		}


		public event Action<bool> IsAuthenticatedChanged;

		public void LoadAchievements(Action<IAchievement[]> callback)
		{
			// Loads all known achievements. Not everyone
			_socialPlatform.LoadAchievements(a =>
			{
				achievementById = a.ToDictionary(b => b.id);
				callback?.Invoke(a);
			});
		}

		public void LoadFromCloud(Action<bool> callback)
		{
			callback?.Invoke(false);
		}

		public void Authenticate(Action<bool> callback)
		{
			if (Authenticated)
			{
				callback?.Invoke(false);
				return;
			}

			_socialPlatform.localUser.Authenticate(success =>
			{
				callback?.Invoke(success);
				IsAuthenticatedChanged?.Invoke(Authenticated);
			});
		}

		public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
		{
			// Not implemented
			callback?.Invoke(false);
		}

		public void ShowAchievementsUI()
		{
			_socialPlatform.ShowAchievementsUI();
		}

		public void IncrementAchievement(string achievementId, double steps, double stepsToComplete,
			Action<bool> callback)
		{
			double stepsAspect = 100.0 / stepsToComplete;
			double percentCompleted = steps * stepsAspect;

			Debug.Log(
				$"iOSSocial IncrementAchievement id: {achievementId}  steps: {steps}  stepsToComplete: {stepsToComplete} stepsAspect:{stepsAspect}");
			IAchievement achievement;
			if (achievementById.ContainsKey(achievementId))
			{
				achievement = achievementById[achievementId];
			}
			else
			{
				achievement = _socialPlatform.CreateAchievement();
				achievement.id = achievementId;
				achievementById.Add(achievement.id, achievement);
			}

			achievement.percentCompleted += percentCompleted;

			Debug.Log($"iOSSocial Achievement {achievement}");
			achievement.ReportProgress(callback);
		}

		public void UnlockAchievement(string achievementId, Action<bool> callback)
		{
			_socialPlatform.ReportProgress(achievementId, 100d, callback);
		}

		public void ShowLeaderboardUI()
		{
			_socialPlatform.ShowLeaderboardUI();
		}

		public ILeaderboard CreateLeaderboard()
		{
			return _socialPlatform.CreateLeaderboard();
		}

		public void ShowLeaderboardUI(string leaderboardId)
		{
			ShowLeaderboardUI();
		}

		public void ReportLeaderboardTime(long score, string leaderboardId, Action<bool> callback)
		{
			// Appstore wants a hundreth of a second, while we calculate in thousandth. 
			score /= 10;
			_socialPlatform.ReportScore(score, leaderboardId, callback);
		}

		public void ReportLeaderboardTime(long score, string leaderboardId, string tag,
			Action<bool> callback)
		{
			ReportLeaderboardTime(score, leaderboardId, callback);
		}

		public void ReportLeaderboardInteger(int score, string leaderboardId, Action<bool> callback)
		{
			_socialPlatform.ReportScore(score, leaderboardId, callback);
		}

		public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
		{
			leaderboard.SetUserFilter(new[] { _socialPlatform.localUser.id });
			leaderboard.LoadScores(callback);
		}

		public IAchievement CreateAchievement()
		{
			return _socialPlatform.CreateAchievement();
		}
	}
}
#endif