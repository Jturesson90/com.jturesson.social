using System;
using System.Collections.Generic;
using System.Linq;
using JTuresson.Social;
using JTuresson.Social.IO;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public class Leaderboards : ILeaderboards
    {
        private readonly ISocialLeaderboards _social;
        private readonly ISession _session;

        public Leaderboards(ISocialLeaderboards social, ISession session)
        {
            _social = social;
            _session = session;
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
                _session.IsAuthenticatedChanged += (bool success) =>
                {
                    if (success)
                    {
                        SavedLeaderboards.Flush(_session, _social);
                    }
                };
            }
            // TODO, if we dont want to use the default UI, we can Loadscores here
        }


        public long GetLocalUserAllTimeHighscore(string leaderboardId, bool isMoreBetter) =>
            SavedLeaderboards.GetLeaderboard(leaderboardId, _session, _social, isMoreBetter)
                .highestScore;

        public void ShowUI() => _social.ShowLeaderboardUI();
        public void ShowUI(string leaderboardId) => _social.ShowLeaderboardUI(leaderboardId);

        public void ReportScore(long score, string leaderboardId, Action<bool> callback,
            bool isMoreBetter) =>
            ReportScore(score, leaderboardId, string.Empty, callback, isMoreBetter);

        public void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback,
            bool isMoreBetter)
        {
            var savedLeaderboard =
                SavedLeaderboards.GetLeaderboard(leaderboardId, _session, _social, isMoreBetter);
            if (_session.Authenticated)
            {
                _social.ReportLeaderboardScore(score, leaderboardId, tag, (bool success) =>
                {
                    callback?.Invoke(success);
                    if (!success)
                    {
                        savedLeaderboard.AddUnpublishedScore(score, DateTime.Now);
                    }
                });
            }
            else
            {
                savedLeaderboard.AddUnpublishedScore(score, DateTime.Now);
            }

            savedLeaderboard.ApplyNewHighscore(score);
            SavedLeaderboards.Save();
        }

        private sealed class SavedLeaderboards
        {
            public static event Action<LeaderboardSaveData> OnLoadScoresComplete;

            static LeaderboardsSave save;

            static readonly string SAVE_PATH = $"{Application.persistentDataPath}/leaderboards.dat";

            public SavedLeaderboards()
            {
                save = new LeaderboardsSave();
            }

            public static LeaderboardSaveData GetLeaderboard(string id, ISession session,
                ISocialLeaderboards social, bool isMoreBetter)
            {
                if (save == null) return null;

                LeaderboardSaveData result = null;

                int saveDateLen = save.data.Length;

                for (int i = 0; i < saveDateLen; i++)
                {
                    if (save.data[i].id.Equals(id))
                        result = save.data[i];
                }

                if (result == null)
                {
                    result = new LeaderboardSaveData(id, isMoreBetter, social);
                    save.Add(result);
                    Save();
                }

                if (result.isMoreBetter != isMoreBetter)
                {
                    result.isMoreBetter = isMoreBetter;
                    Save();
                }


                if (session.Authenticated && !result.LoadedScores)
                {
                    social.LoadUserLeaderboardScore(result.GetLeaderboard(), (bool success) =>
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
                if (!session.Authenticated) return;

                var saveDateLen = save.data.Length;
                for (var i = 0; i < saveDateLen; i++)
                {
                    LeaderboardSaveData lbsd = save.data[i];
                    lbsd.unpublishedScores = lbsd.unpublishedScores
                        .Where(a => DateTime.FromFileTime(a.date) > DateTime.Now.AddDays(-7)).ToArray();
                    var unpublishedLen = lbsd.unpublishedScores.Length;
                    for (var j = 0; j < unpublishedLen; j++)
                    {
                        var us = lbsd.unpublishedScores[j];
                        social.ReportLeaderboardScore(us.score, lbsd.id, (bool success) => { });
                    }

                    lbsd.ClearUnpublishedScores();
                }

                Save();
            }

            public static void Save()
            {
                if (SAVE_PATH.Equals(string.Empty))
                {
                    Debug.Log("COULD NOT SAVE, NEEDS LOAD FIRST");
                    return;
                }

                try
                {
                    EasySerializer.SerializeObjectToFile(save, SAVE_PATH);
                }
                catch
                {
                    EasySerializer.RemoveFile(SAVE_PATH);
                }
            }

            public static void Load()
            {
                try
                {
                    if (!(EasySerializer.DeserializeObjectFromFile(SAVE_PATH) is LeaderboardsSave
                            savedScore))
                    {
                        savedScore = new LeaderboardsSave();
                    }

                    save = savedScore;
                }
                catch
                {
                    save = new LeaderboardsSave();
                    EasySerializer.RemoveFile(SAVE_PATH);
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
            public bool LoadedScores { get; set; }
            public readonly string id;
            public bool isMoreBetter;
            public string tag;
            public long highestScore;
            [NonSerialized] private ILeaderboard _leaderboard;
            public UnpublishedScores[] unpublishedScores = new UnpublishedScores[0];

            private ISocialLeaderboards _socialLeaderboard;

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

            public ILeaderboard GetLeaderboard()
            {
                if (_leaderboard != null) return _leaderboard;

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
                    new UnpublishedScores(score, time.ToFileTime())
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