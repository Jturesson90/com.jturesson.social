using System;

namespace JTuresson.Social
{
    public interface ILeaderboards
    {
        void ReportScore(long score, string leaderboardId, Action<bool> callback, bool isMoreBetter = false);
        void ReportScore(long score, string leaderboardId, string tag, Action<bool> callback, bool isMoreBetter = false);
        void ShowUI();
        void ShowUI(string leaderboardId);
        long GetLocalUserAllTimeHighscore(string leaderboardId, bool isMoreBetter = false);
    }
}