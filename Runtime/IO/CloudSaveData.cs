using System;
using UnityEngine;

namespace JTuresson.Social.IO
{
    [Serializable]
    public class CloudSaveData
    {
        public double totalPlayingTime;

        public static CloudSaveData FromBytes(byte[] data) => data != null
            ? FromString(System.Text.ASCIIEncoding.Default.GetString(data))
            : default;

        private static CloudSaveData FromString(string s) => JsonUtility.FromJson<CloudSaveData>(s);
        public byte[] ToBytes() => System.Text.ASCIIEncoding.Default.GetBytes(ToString());
        public override string ToString() => JsonUtility.ToJson(this, false);
    }
}