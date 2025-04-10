using System;

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

		public void ReportInteger(int score, string leaderboardId, Action<bool> callback, bool isMoreBetter = false,
			string tag = default)
		{
			if (_session.Authenticated)
			{
				_social.ReportLeaderboardInteger(score, leaderboardId, callback);
			}
			else
			{
				callback?.Invoke(false);
			}
		}

		public void ReportTime(long score, string leaderboardId, Action<bool> callback, bool isMoreBetter = false,
			string tag = default)
		{
			if (_session.Authenticated)
			{
				_social.ReportLeaderboardTime(score, leaderboardId, tag, success => { callback?.Invoke(success); });
			}
			else
			{
				callback?.Invoke(false);
			}
		}

		public void ShowUI()
		{
			_social.ShowLeaderboardUI();
		}

		public void ShowUI(string leaderboardId)
		{
			_social.ShowLeaderboardUI(leaderboardId);
		}

		public void Initialize()
		{
		}
	}
}