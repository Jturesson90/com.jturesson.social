using UnityEngine;

namespace JTuresson.Social.ScriptableObjects
{
    public class SocialSettingsSO : ScriptableObject
    {
        [Header("Social services Settings")] public bool cloudSave = false;
        public bool leaderboards = false;
        public bool achievements = false;

        [Header("Store settings")] public string cloudFileName = "cloud_save1";
        public string storeName = "Store name";

        public override string ToString()
        {
            return
                $"Leaderboard enabled : {leaderboards}, Achievements enabled : {achievements} - CloudSave enabled : {cloudSave} - StoreName: {storeName}";
        }
    }
}