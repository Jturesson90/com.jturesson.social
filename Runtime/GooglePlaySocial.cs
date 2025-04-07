#if UNITY_ANDROID
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using JTuresson.Social.ScriptableObjects;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
	public class GooglePlaySocial : ISocialService
	{
		private readonly string cloudFileName;

		private readonly bool debugMode;
		private bool _currentlyLoadingFromCloud;
		private bool _currentlySavingToCloud;
		private Action<bool> _loadCallback;

		private Action<bool> _saveCallback;
		private readonly PlayGamesPlatform _social;

		public GooglePlaySocial(SocialAndroidSettingsSO settings, PlayGamesPlatform social)
		{
			Debug.Log("Google Play Social - Instantiating with settings " + settings);
			LeaderboardsEnabled = settings.leaderboards;
			AchievementsEnabled = settings.achievements;
			CloudSaveEnabled = settings.cloudSave;
			cloudFileName = settings.cloudFileName;
			StoreName = settings.storeName;
			debugMode = settings.debugLog;
			_social = social;

			if (_social == null)
			{
				_social = PlayGamesPlatform.Activate();
			}

			if (debugMode && _social == null)
			{
				Debug.LogError("Google Play Social - Social is still null");
			}
		}

		public string Name => _social.localUser.userName;
		public bool Authenticated => _social.IsAuthenticated();
		public RuntimePlatform Platform => RuntimePlatform.Android;
		public event Action<bool> IsAuthenticatedChanged;
		public bool UserCanSign => true;
		public string UserName => _social.GetUserDisplayName();
		public string StoreName { get; }

		public byte[] CloudData { get; private set; }

		public bool CloudSaveEnabled { get; }
		public bool AchievementsEnabled { get; }
		public bool LeaderboardsEnabled { get; }

		public string LocalUserId => _social.localUser.id;

		public void Initialize()
		{
		}

		public void LoadFromCloud(Action<bool> callback)
		{
			if (_currentlyLoadingFromCloud || !Authenticated)
			{
				Debug.LogWarning("GooglePlaySocial loading or is not LoggedIn");
				LoadComplete(false);
				return;
			}

			_currentlyLoadingFromCloud = true;
			_loadCallback = callback;

			_social.SavedGame.OpenWithAutomaticConflictResolution(cloudFileName,
				DataSource.ReadCacheOrNetwork,
				ConflictResolutionStrategy.UseLongestPlaytime,
				SavedGameOpened);
		}


		public void Authenticate(Action<bool> callback)
		{
			if (debugMode)
			{
				Debug.Log("Google Play Social - Authenticating");
			}

			if (Authenticated)
			{
				if (debugMode)
				{
					Debug.Log("Google Play Social - Authenticating - Already Authenticated");
				}

				callback?.Invoke(false);
				return;
			}

			_social.Authenticate(status =>
			{
				if (debugMode)
				{
					Debug.Log("Google Play Social - Authenticating - Authenticating done with status " +
					          status);
				}

				callback?.Invoke(status == SignInStatus.Success);
				IsAuthenticatedChanged?.Invoke(Authenticated);
			});
		}


		public void SaveGame(byte[] data, TimeSpan playedTime, Action<bool> callback)
		{
			if (_currentlySavingToCloud)
			{
				SaveComplete(false);
			}

			_saveCallback = callback;
			_currentlySavingToCloud = true;

			_social.SavedGame.OpenWithAutomaticConflictResolution(cloudFileName,
				DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime,
				(status, game) =>
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
							(committedStatus, committedGame) =>
							{
								Debug.LogWarning("GooglePlaySocial SavedGameRequestStatus " +
								                 committedStatus + " " +
								                 committedGame.Description + " committedGame: " +
								                 committedGame.Filename);
								SaveComplete(committedStatus == SavedGameRequestStatus.Success);
							}
						);
				}
			);
		}

		public void UnlockAchievement(string achievementId, Action<bool> callback)
		{
			_social.UnlockAchievement(achievementId, callback);
		}

		public void IncrementAchievement(string achievementId, double steps, double stepsRatio,
			Action<bool> callback)
		{
			_social.IncrementAchievement(achievementId, (int)steps, callback);
		}

		public void LoadAchievements(Action<IAchievement[]> callback)
		{
			_social.LoadAchievements(callback);
		}

		public void ShowAchievementsUI()
		{
			if (debugMode)
			{
				Debug.Log("Google Play Social - ShowAchievementsUI");
			}

			_social.ShowAchievementsUI();
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
			_social.ShowLeaderboardUI(leaderboardId, status =>
			{
				if (status < 0)
				{
					Debug.LogError("ShowLeaderboardUI " + status);
				}
			});
		}

		public void ReportLeaderboardTime(long score, string leaderboardId, Action<bool> callback)
		{
			_social.ReportScore(score, leaderboardId, callback);
		}

		public void ReportLeaderboardTime(long score, string leaderboardId, string tag,
			Action<bool> callback)
		{
			_social.ReportScore(score, leaderboardId, tag, callback);
		}

		public void ReportLeaderboardInteger(int score, string leaderboardId, Action<bool> callback)
		{
			ReportLeaderboardTime(score, leaderboardId, callback);
		}

		public void LoadUserLeaderboardScore(ILeaderboard leaderboard, Action<bool> callback)
		{
			leaderboard.SetUserFilter(new[] { _social.localUser.id });
			leaderboard.LoadScores(callback);
		}


		private void SaveComplete(bool success)
		{
			_currentlySavingToCloud = false;
			if (_saveCallback != null)
			{
				_saveCallback.Invoke(success);
			}

			_saveCallback = null;
		}


		private void LoadComplete(bool success)
		{
			_currentlyLoadingFromCloud = false;
			if (_loadCallback != null)
			{
				_loadCallback.Invoke(success);
			}

			_loadCallback = null;
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

		public IAchievement CreateAchievement()
		{
			return _social.CreateAchievement();
		}
	}
}
#endif