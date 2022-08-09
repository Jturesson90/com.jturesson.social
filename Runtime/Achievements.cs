//#define DEBUG_ACHIEVEMENTS

using System;
using System.Collections.Generic;
using System.Linq;
using JTuresson.Social;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public class Achievements : IAchievements
    {
        private PendingAchievements _pendingAchievements;
        private Dictionary<string, IAchievement> _unlockedAchievements;

        private readonly ISocialAchievements _socialAchievements;
        private readonly ISession _session;

        public Achievements(ISocialAchievements socialAchievements, ISession session)
        {
            _socialAchievements = socialAchievements;
            _session = session;
            _pendingAchievements = new PendingAchievements();
            _unlockedAchievements = new Dictionary<string, IAchievement>();
        }

        private void Session_IsAuthenticatedChanged(bool isLoggedIn)
        {
            if (isLoggedIn)
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

        ~Achievements()
        {
            SaveToDisk();
        }

        private void FlushAchievements()
        {
            if (!_session.Authenticated) return;

            foreach (var pending in _pendingAchievements.pending)
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

        public void Unlock(string id, Action<bool> callback = null)
        {
            if (!_socialAchievements.AchievementsEnabled) return;
            var s = new DroleAchievement(id);
            if (!_session.Authenticated)
            {
                AddPendingAchievement(s);
                return;
            }

            _socialAchievements.UnlockAchievement(id, (bool success) =>
            {
                if (success)
                    RemovePendingAchievement(s);
                else
                    AddPendingAchievement(s);

                callback?.Invoke(success);
            });
        }

        public void Increment(string id, double steps, double stepsRatio, Action<bool> callback = null)
        {
            if (!_socialAchievements.AchievementsEnabled) return;
            var s = new DroleAchievement(id, steps, stepsRatio);
            if (!_session.Authenticated)
            {
                AddPendingAchievement(s);
                return;
            }

            _socialAchievements.IncrementAchievement(id, s.steps, s.stepsToComplete, (bool success) =>
            {
                if (success)
                    RemovePendingAchievement(s);
                else
                    AddPendingAchievement(s);

                callback?.Invoke(success);
            });
        }

        private void RemovePendingAchievement(DroleAchievement pendingAchievement)
        {
            if (_pendingAchievements != null)
            {
                _pendingAchievements.RemoveAchievement(pendingAchievement);
            }
        }

        private void AddPendingAchievement(DroleAchievement pendingAchievement)
        {
            if (_pendingAchievements != null)
            {
                _pendingAchievements.AddAchievement(pendingAchievement);
            }
        }

        public void ShowUI()
        {
            if (!_socialAchievements.AchievementsEnabled) return;
            _socialAchievements.ShowAchievementsUI();
        }

        private void LoadAchievements()
        {
            _unlockedAchievements.Clear();
            _socialAchievements.LoadAchievements(achievements =>
            {
                _unlockedAchievements = new Dictionary<string, IAchievement>();
                foreach (var achievement in achievements)
                {
                    if (achievement.completed)
                    {
                        _unlockedAchievements.Add(achievement.id, achievement);
                    }
                }

                if (_pendingAchievements != null)
                    _pendingAchievements.RemoveAllWithId(_unlockedAchievements.Keys.ToArray());

                FlushAchievements();
            });
            SaveToDisk();
        }

        const string saveKey = "pend";

        private void SaveToDisk()
        {
            var json = _pendingAchievements.ToString();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }

        private void LoadFromDisk()
        {
            var s = PlayerPrefs.GetString(saveKey, string.Empty);
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
        public List<DroleAchievement> pending;

        public static PendingAchievements FromString(string s) =>
            JsonUtility.FromJson<PendingAchievements>(s);

        public override string ToString() => JsonUtility.ToJson(this, false);

        public PendingAchievements()
        {
            pending = new List<DroleAchievement>();
        }

        public void RemoveAllWithId(string[] ids)
        {
            pending.RemoveAll((item) => ids.Contains(item.id));
        }

        internal void RemoveAchievement(DroleAchievement pendingAchievement)
        {
            if (!pending.Contains(pendingAchievement))
            {
                pending.Remove(pendingAchievement);
            }
        }

        internal void AddAchievement(DroleAchievement pendingAchievement)
        {
            if (!pending.Contains(pendingAchievement))
            {
                pending.Add(pendingAchievement);
            }
        }
    }
}