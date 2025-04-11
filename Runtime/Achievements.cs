//#define DEBUG_ACHIEVEMENTS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
	public class Achievements : IAchievements
	{
		private const string SaveKey = "pend";
		private readonly Dictionary<string, IAchievement> _allAchievements;
		private readonly ISession _session;

		private readonly ISocialAchievements _socialAchievements;
		private readonly Dictionary<string, IAchievement> _unlockedAchievements;
		private PendingAchievements _pendingAchievements;

		public Achievements(ISocialAchievements socialAchievements, ISession session)
		{
			_socialAchievements = socialAchievements;
			_session = session;
			_pendingAchievements = new PendingAchievements();
			_unlockedAchievements = new Dictionary<string, IAchievement>();
			_allAchievements = new Dictionary<string, IAchievement>();
		}

		public IReadOnlyList<string> UnlockedAchievementIds => _unlockedAchievements.Keys.ToList();

		public IReadOnlyList<string> AllAchievementIds => _allAchievements.Keys.ToList();

		public event Action<IReadOnlyList<string>> UnlockedAchievementsChanged;
		public event Action<IReadOnlyList<string>> AllAchievementsChanged;

		public void Unlock(string id, Action<bool> callback = null)
		{
			if (!_socialAchievements.AchievementsEnabled)
			{
				return;
			}

			var s = new DroleAchievement(id);
			if (!_session.Authenticated)
			{
				AddPendingAchievement(s);
				return;
			}

			_socialAchievements.UnlockAchievement(id, success =>
			{
				if (success)
				{
					RemovePendingAchievement(s);
				}
				else
				{
					AddPendingAchievement(s);
				}

				callback?.Invoke(success);
			});
		}

		public void Increment(string id, double steps, double stepsRatio, Action<bool> callback = null)
		{
			if (!_socialAchievements.AchievementsEnabled)
			{
				return;
			}

			var s = new DroleAchievement(id, steps, stepsRatio);
			if (!_session.Authenticated)
			{
				AddPendingAchievement(s);
				return;
			}

			_socialAchievements.IncrementAchievement(id, s.steps, s.stepsToComplete, success =>
			{
				if (success)
				{
					RemovePendingAchievement(s);
				}
				else
				{
					AddPendingAchievement(s);
				}

				callback?.Invoke(success);
			});
		}

		public void ShowUI()
		{
			if (!_socialAchievements.AchievementsEnabled)
			{
				return;
			}

			_socialAchievements.ShowAchievementsUI();
		}

		private void Session_IsAuthenticatedChanged(bool authenticated)
		{
			if (authenticated)
			{
				LoadAchievements();
			}
		}

		public void Initialize()
		{
			LoadFromDisk();
			if (_session.Authenticated)
			{
				LoadAchievements();
			}
			else
			{
				_session.IsAuthenticatedChanged += Session_IsAuthenticatedChanged;
			}
		}

		public void Save()
		{
			SaveToDisk();
		}

		private void FlushAchievements()
		{
			if (!_session.Authenticated)
			{
				return;
			}

			foreach (DroleAchievement pending in _pendingAchievements.Pending)
			{
				if (pending.hasIncrement)
				{
					Increment(pending.id, pending.steps, pending.stepsToComplete);
				}
				else
				{
					Unlock(pending.id);
				}
			}
		}

		private void RemovePendingAchievement(DroleAchievement pendingAchievement)
		{
			if (_pendingAchievements != null)
			{
				_pendingAchievements.RemoveAchievement(pendingAchievement);
				UpdateAchievements(pendingAchievement.id);
			}
		}

		private void AddPendingAchievement(DroleAchievement pendingAchievement)
		{
			if (_pendingAchievements != null)
			{
				_pendingAchievements.AddAchievement(pendingAchievement);
				UpdateAchievements(pendingAchievement.id);
			}
		}

		private void UpdateAchievements(string id)
		{
			if (_allAchievements.ContainsKey(id))
			{
				if (!_unlockedAchievements.ContainsKey(id))
				{
					_unlockedAchievements.Add(id, _unlockedAchievements[id]);
					UnlockedAchievementsChanged?.Invoke(UnlockedAchievementIds);
				}
			}
		}

		private void LoadAchievements()
		{
			_unlockedAchievements.Clear();
			_allAchievements.Clear();
			_socialAchievements.LoadAchievements(achievements =>
			{
				foreach (IAchievement achievement in achievements)
				{
					_allAchievements.Add(achievement.id, achievement);
					if (achievement.completed)
					{
						_unlockedAchievements.Add(achievement.id, achievement);
					}
				}

				if (_pendingAchievements != null)
				{
					_pendingAchievements.RemoveAllWithId(_unlockedAchievements.Keys.ToArray());
				}

				FlushAchievements();
				AllAchievementsChanged?.Invoke(AllAchievementIds);
				UnlockedAchievementsChanged?.Invoke(UnlockedAchievementIds);
			});
			SaveToDisk();
		}

		private void SaveToDisk()
		{
			var json = _pendingAchievements.ToString();
			PlayerPrefs.SetString(SaveKey, json);
			PlayerPrefs.Save();
		}

		private void LoadFromDisk()
		{
			string s = PlayerPrefs.GetString(SaveKey, string.Empty);
			if (s == null || s.Trim().Length == 0)
			{
				_pendingAchievements = new PendingAchievements();
			}
			else
			{
				_pendingAchievements = PendingAchievements.FromString(s);
			}
		}
	}

	[Serializable]
	public class PendingAchievements
	{
		[SerializeField] private List<DroleAchievement> pending = new();
		public IReadOnlyList<DroleAchievement> Pending => pending;

		public static PendingAchievements FromString(string s)
		{
			return JsonUtility.FromJson<PendingAchievements>(s);
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this, false);
		}

		public void RemoveAllWithId(string[] ids)
		{
			pending.RemoveAll(item => ids.Contains(item.id));
		}

		public void RemoveAchievement(DroleAchievement pendingAchievement)
		{
			if (pending.Contains(pendingAchievement))
			{
				pending.Remove(pendingAchievement);
			}
		}

		public void AddAchievement(DroleAchievement pendingAchievement)
		{
			if (!pending.Contains(pendingAchievement))
			{
				pending.Add(pendingAchievement);
			}
		}
	}
}