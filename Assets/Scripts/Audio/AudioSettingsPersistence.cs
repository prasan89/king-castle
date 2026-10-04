using UnityEngine;

namespace KingSmash.Audio
{
    public static class AudioSettingsPersistence
    {
        public const string MusicVolumeKey = "audio_music_vol";
        public const string SfxVolumeKey   = "audio_sfx_vol";
        public const string MutedKey       = "audio_muted";

        public static void Save(float music, float sfx, bool muted)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(music));
            PlayerPrefs.SetFloat(SfxVolumeKey,   Mathf.Clamp01(sfx));
            PlayerPrefs.SetInt(MutedKey,          muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static (float music, float sfx, bool muted) Load()
        {
            float music = PlayerPrefs.GetFloat(MusicVolumeKey, 0.6f);
            float sfx   = PlayerPrefs.GetFloat(SfxVolumeKey,   0.8f);
            bool  muted = PlayerPrefs.GetInt(MutedKey, 0) != 0;
            return (music, sfx, muted);
        }
    }
}
