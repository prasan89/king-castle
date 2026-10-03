using System.Collections.Generic;
using UnityEngine;
using KingSmash.Services;

namespace KingSmash.Core
{
    public class AudioManager : IAudioService
    {
        private float _musicVolume = 0.6f;
        private float _sfxVolume = 0.8f;
        private readonly Dictionary<string, AudioClip> _clipCache = new();

        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            GameLogger.Debug("AudioManager", $"Music volume set to {_musicVolume}");
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            GameLogger.Debug("AudioManager", $"SFX volume set to {_sfxVolume}");
        }

        public void PlaySfx(string clipName)
        {
            GameLogger.Debug("AudioManager", $"PlaySfx: {clipName}");
            // AudioSource management implemented when MonoBehaviour runner is wired
        }

        public void PlayMusic(string clipName, bool loop = true)
        {
            GameLogger.Debug("AudioManager", $"PlayMusic: {clipName} loop={loop}");
        }

        public void StopMusic()
        {
            GameLogger.Debug("AudioManager", "StopMusic");
        }
    }
}
