using System;

namespace JTuresson.Social
{
	public interface ILeaderboards
	{
		void ReportInteger(int score, string leaderboardId, Action<bool> callback, bool isMoreBetter = false,
			string tag = default);

		void ReportTime(long score, string leaderboardId, Action<bool> callback, bool isMoreBetter = false,
			string tag = default);

		void ShowUI();
		void ShowUI(string leaderboardId);
	}
}