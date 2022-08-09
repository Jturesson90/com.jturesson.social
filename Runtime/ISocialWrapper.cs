using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public interface ISocialWrapper
    {
        public ILocalUser LocalUser { get; }
        public void ShowLeaderboardUI();
        public void ShowAchievementsUI();
    }
}