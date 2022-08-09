namespace Drolegames.SocialService
{
    [System.Flags]
    public enum SocialFlags
    {
        EnableSavedGames = 1 << 1, // 1
        DebugLog = 1 << 2, // 2
        RequestEmail = 1 << 4, // 4
        EnableAchievements = 1 << 8, //
        EnableLeaderboards = 1 << 16,
        ALL = EnableSavedGames | DebugLog | RequestEmail | EnableAchievements | EnableLeaderboards
    }
}