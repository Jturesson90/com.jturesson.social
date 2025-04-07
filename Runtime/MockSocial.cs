using System;
using System.Text;
using System.Threading.Tasks;
using JTuresson.Social.IO;
using JTuresson.Social.ScriptableObjects;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Range = UnityEngine.SocialPlatforms.Range;

namespace JTuresson.Social
{
	public class MockSocial : ISocialService
	{
		private readonly string _cloudFileName;

		private readonly IFileManager _fileManager;

		private readonly ISocialPlatform _social;

		private bool _isAuthenticated;

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

		public RuntimePlatform Platform => RuntimePlatform.WindowsEditor;

		public bool UserCanSign => true;

		public bool Authenticated
		{
			get => _isAuthenticated;
			private set
			{
				if (_isAuthenticated == value)
				{
					return;
				}

				_isAuthenticated = value;
				IsAuthenticatedChanged?.Invoke(value);
			}
		}

		public event Action<bool> IsAuthenticatedChanged;
		public string UserName => Authenticated ? LocalUserId : string.Empty;
		public string StoreName { get; }

		public byte[] CloudData { get; private set; }

		public bool CloudSaveEnabled { get; }
		public bool AchievementsEnabled { get; }

		public bool LeaderboardsEnabled { get; }

		public string LocalUserId { get; }

		public void Initialize()
		{
			Authenticated = false;
		}

		public void Authenticate(Action<bool> callback)
		{
			if (Authenticated)
			{
				callback?.Invoke(false);
				return;
			}

			_social.localUser.Authenticate(success => { });
			callback?.Invoke(Authenticated = true);
			IsAuthenticatedChanged?.Invoke(Authenticated);
		}

		public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
		{
			var success = false;
			if (!CloudSaveEnabled)
			{
				callback?.Invoke(success);
				return;
			}

			CloudData = data;
			try
			{
				_fileManager.WriteToFile($"{_cloudFileName}.txt",
					Encoding.Default.GetString(data));
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
				_fileManager.LoadFromFile($"{_cloudFileName}.txt", out string json);
				CloudData = Encoding.Default.GetBytes(json);
				success = true;
			}
			catch
			{
				success = false;
			}

			callback?.Invoke(success);
			// UseDelay(loginDelay, () => callback?.Invoke(success));
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
			callback(new IAchievement[]
			{
				new AchievementMock
				{
					id = "1",
				},
				new AchievementMock
				{
					id = "2",
				},
				new AchievementMock
				{
					id = "3",
				},
			});
		}

		public void ShowLeaderboardUI()
		{
			_social.ShowLeaderboardUI();
		}

		public ILeaderboard CreateLeaderboard()
		{
			return new LeaderboardMock();
		}

		public void ShowLeaderboardUI(string leaderboardId)
		{
			ShowLeaderboardUI();
		}

		public void ReportLeaderboardTime(long score, string leaderboardId, Action<bool> callback)
		{
			Debug.LogWarning("ReportLeaderboardScore not implemented in Editor");
			callback?.Invoke(false);
		}

		public void ReportLeaderboardTime(long score, string leaderboardId, string tag,
			Action<bool> callback)
		{
			ReportLeaderboardTime(score, leaderboardId, callback);
		}

		public void ReportLeaderboardInteger(int score, string leaderboardId, Action<bool> callback)
		{
			callback?.Invoke(true);
		}

		public void ShowAchievementsUI()
		{
			_social.ShowLeaderboardUI();
		}

		public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
		{
			Debug.LogWarning("LoadUserLeaderboardScore not implemented in Editor");
			callback?.Invoke(false);
		}

		public IAchievement CreateAchievement()
		{
			throw new NotImplementedException();
		}

		// TODO Fix, call back on main thread
		private void UseDelay(float time, Action callback)
		{
			Task.Run(async delegate
			{
				await Task.Delay(TimeSpan.FromSeconds(time));
				callback?.Invoke();
			});
		}

		private class LeaderboardMock : ILeaderboard
		{
			public void SetUserFilter(string[] userIDs)
			{
			}

			public void LoadScores(Action<bool> callback)
			{
			}

			public bool loading { get; }
			public string id { get; set; }
			public UserScope userScope { get; set; }
			public Range range { get; set; }
			public TimeScope timeScope { get; set; }
			public IScore localUserScore { get; }
			public uint maxRange { get; }
			public IScore[] scores { get; }
			public string title { get; }
		}

		private class AchievementMock : IAchievement
		{
			public void ReportProgress(Action<bool> callback)
			{
				callback?.Invoke(false);
			}

			public string id { get; set; }
			public double percentCompleted { get; set; }
			public bool completed { get; }
			public bool hidden { get; }
			public DateTime lastReportedDate { get; }
		}
	}
}