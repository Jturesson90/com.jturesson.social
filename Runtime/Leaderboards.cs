using System;
using System.Collections.Generic;
using System.Linq;
using JTuresson.Social.IO;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
	public class Leaderboards : ILeaderboards
	{
		private readonly ISession _session;
		private readonly ISocialLeaderboards _social;

		public Leaderboards(ISocialLeaderboards social, ISession session)
		{
			_social = social;
			_session = session;
		}


		public long GetLocalUserAllTimeHighscore(string leaderboardId, bool isMoreBetter)
		{
			return SavedLeaderboards.GetLeaderboard(leaderboardId, _session, _social, isMoreBetter)
				.highestScore;
		}

		public void ShowUI()
		{
			_social.ShowLeaderboardUI();
		}

		public void ShowUI(string leaderboardId)
		{
			_social.ShowLeaderboardUI(leaderboardId);
		}

		public void ReportScore(long score, string leaderboardId, Action<bool> callback, bool isMoreBetter)
		{
			ReportScore(score, leaderboardId, string.Empty, callback, isMoreBetter);
		}

		public void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback,
			bool isMoreBetter)
		{
			LeaderboardSaveData savedLeaderboard =
				SavedLeaderboards.GetLeaderboard(leaderboardId, _session, _social, isMoreBetter);
			if (_session.Authenticated)
			{
				_social.ReportLeaderboardTime(score, leaderboardId, tag, success =>
				{
					callback?.Invoke(success);
					if (!success)
					{
						savedLeaderboard?.AddUnpublishedScore(score, DateTime.Now);
					}
				});
			}
			else
			{
				savedLeaderboard?.AddUnpublishedScore(score, DateTime.Now);
			}

			savedLeaderboard?.ApplyNewHighscore(score);
			SavedLeaderboards.Save();
		}

		public void Initialize()
		{
			SavedLeaderboards.Load();
			if (_session.Authenticated)
			{
				SavedLeaderboards.Flush(_session, _social);
			}
			else
			{
				_session.IsAuthenticatedChanged += success =>
				{
					if (success)
					{
						SavedLeaderboards.Flush(_session, _social);
					}
				};
			}
			// TODO, if we dont want to use the default UI, we can Loadscores here
		}

		private sealed class SavedLeaderboards
		{
			private static LeaderboardsSave _save;

			private static readonly string SavePath = $"{Application.persistentDataPath}/leaderboards.dat";

			public SavedLeaderboards()
			{
				_save = new LeaderboardsSave();
			}

			public static event Action<LeaderboardSaveData> OnLoadScoresComplete;

			public static LeaderboardSaveData GetLeaderboard(string id, ISession session,
				ISocialLeaderboards social, bool isMoreBetter)
			{
				if (_save == null)
				{
					return null;
				}

				LeaderboardSaveData result = null;

				int saveDateLen = _save.data.Length;

				for (var i = 0; i < saveDateLen; i++)
				{
					if (_save.data[i].id.Equals(id))
					{
						result = _save.data[i];
					}
				}

				if (result == null)
				{
					result = new LeaderboardSaveData(id, isMoreBetter, social);
					_save.Add(result);
					Save();
				}

				if (result.isMoreBetter != isMoreBetter)
				{
					result.isMoreBetter = isMoreBetter;
					Save();
				}


				if (session.Authenticated && !result.LoadedScores)
				{
					social.LoadUserLeaderboardScore(result.GetLeaderboard(), success =>
					{
						if (success)
						{
							result.LoadedScores = true;
							result.ApplyNewHighscore(result.GetLeaderboard().localUserScore.value);
							OnLoadScoresComplete?.Invoke(result);
						}
					});
				}

				return result;
			}

			public static void Flush(ISession session, ISocialLeaderboards social)
			{
				if (!session.Authenticated)
				{
					return;
				}

				int saveDateLen = _save.data.Length;
				for (var i = 0; i < saveDateLen; i++)
				{
					LeaderboardSaveData lbsd = _save.data[i];
					lbsd.unpublishedScores = lbsd.unpublishedScores
						.Where(a => DateTime.FromFileTime(a.date) > DateTime.Now.AddDays(-7)).ToArray();
					int unpublishedLen = lbsd.unpublishedScores.Length;
					for (var j = 0; j < unpublishedLen; j++)
					{
						UnpublishedScores us = lbsd.unpublishedScores[j];
						social.ReportLeaderboardTime(us.score, lbsd.id, success => { });
					}

					lbsd.ClearUnpublishedScores();
				}

				Save();
			}

			public static void Save()
			{
				if (SavePath.Equals(string.Empty))
				{
					Debug.Log("COULD NOT SAVE, NEEDS LOAD FIRST");
					return;
				}

				try
				{
					EasySerializer.SerializeObjectToFile(_save, SavePath);
				}
				catch
				{
					EasySerializer.RemoveFile(SavePath);
				}
			}

			public static void Load()
			{
				try
				{
					if (!(EasySerializer.DeserializeObjectFromFile(SavePath) is LeaderboardsSave
						    savedScore))
					{
						savedScore = new LeaderboardsSave();
					}

					_save = savedScore;
				}
				catch (Exception e)
				{
					Debug.LogWarning("Exception " + e.Message);
					_save = new LeaderboardsSave();
					EasySerializer.RemoveFile(SavePath);
				}
			}

			[Serializable]
			public class LeaderboardsSave
			{
				public LeaderboardSaveData[] data;

				public LeaderboardsSave()
				{
					data = Array.Empty<LeaderboardSaveData>();
				}

				internal void Add(LeaderboardSaveData result)
				{
					data = data.Append(result).ToArray();
				}
			}
		}

		[Serializable]
		public class LeaderboardSaveData
		{
			public bool isMoreBetter;
			public string tag;
			public long highestScore;
			public UnpublishedScores[] unpublishedScores = new UnpublishedScores[0];
			public readonly string id;
			[NonSerialized] private ILeaderboard _leaderboard;

			[NonSerialized] private ISocialLeaderboards _socialLeaderboard;

			public LeaderboardSaveData(string id, bool isMoreBetter,
				ISocialLeaderboards socialLeaderboard)
			{
				this.isMoreBetter = isMoreBetter;
				this.id = id;

				unpublishedScores = Array.Empty<UnpublishedScores>();
				_socialLeaderboard = socialLeaderboard;
				_leaderboard = socialLeaderboard.CreateLeaderboard();
				_leaderboard.id = id;
			}

			public bool LoadedScores { get; set; }

			public ILeaderboard GetLeaderboard()
			{
				if (_leaderboard != null)
				{
					return _leaderboard;
				}

				_leaderboard = _socialLeaderboard.CreateLeaderboard();
				_leaderboard.id = id;

				return _leaderboard;
			}

			public void ClearUnpublishedScores()
			{
				unpublishedScores = Array.Empty<UnpublishedScores>();
			}

			public void ApplyNewHighscore(long score)
			{
				if (isMoreBetter)
				{
					if (score > highestScore)
					{
						highestScore = score;
					}
				}
				else
				{
					if (score < highestScore)
					{
						highestScore = score;
					}
				}
			}

			public void AddUnpublishedScore(long score, DateTime time)
			{
				var u = new List<UnpublishedScores>(unpublishedScores)
				{
					new(score, time.ToFileTime()),
				};
				unpublishedScores = u.ToArray();
			}
		}

		[Serializable]
		public class UnpublishedScores
		{
			public long date;
			public long score;

			public UnpublishedScores(long score, long date)
			{
				this.score = score;
				this.date = date;
			}
		}
	}
}