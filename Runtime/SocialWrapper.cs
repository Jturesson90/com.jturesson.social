using UnityEngine.SocialPlatforms;

namespace JTuresson.Social
{
    public class SocialWrapper : ISocialWrapper
    {
        public ILocalUser LocalUser => UnityEngine.Social.localUser;
        public void ShowAchievementsUI() => UnityEngine.Social.ShowAchievementsUI();
        public void ShowLeaderboardUI() => UnityEngine.Social.ShowLeaderboardUI();
    }
}