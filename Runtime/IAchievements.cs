using System;

namespace JTuresson.Social
{
    public interface IAchievements
    {
        void Unlock(string achievementId, Action<bool> callback);

        void Increment(string achievementId, double steps, double stepsToComplete,
            Action<bool> callback);

        void ShowUI();
    }
}