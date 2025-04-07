using System;
using System.Collections.Generic;

namespace JTuresson.Social
{
	public interface IAchievements
	{
		IReadOnlyList<string> UnlockedAchievementIds { get; }
		IReadOnlyList<string> AllAchievementIds { get; }
		event Action<IReadOnlyList<string>> UnlockedAchievementsChanged;
		event Action<IReadOnlyList<string>> AllAchievementsChanged;

		void Unlock(string achievementId, Action<bool> callback);

		void Increment(string achievementId, double steps, double stepsToComplete,
			Action<bool> callback);

		void ShowUI();
	}
}