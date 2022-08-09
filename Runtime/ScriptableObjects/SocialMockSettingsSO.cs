using UnityEngine;

namespace JTuresson.Social.ScriptableObjects
{
    [CreateAssetMenu(fileName = "", menuName = "JTuresson/Social/Mock")]
    public class SocialMockSettingsSO : SocialSettingsSO
    {
        [Header("Mock settings")] public string userName = "Mock";
        [Min(0)] public float loginDelay = 1.5f;
    }
}