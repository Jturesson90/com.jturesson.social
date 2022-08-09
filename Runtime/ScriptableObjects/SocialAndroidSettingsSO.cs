using UnityEngine;

namespace JTuresson.Social.ScriptableObjects
{
    [CreateAssetMenu(fileName = "", menuName = "JTuresson/Social/Android")]
    public class SocialAndroidSettingsSO : SocialSettingsSO
    {
        [Header("Android Settings")] public bool debugLog = false;
    }
}