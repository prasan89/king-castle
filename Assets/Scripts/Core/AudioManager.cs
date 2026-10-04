using System.Collections.Generic;
using UnityEngine;
using KingSmash.Services;
using KingSmash.Audio;

namespace KingSmash.Core
{
    /// <summary>
    /// Lightweight stub AudioManager used until AudioService MonoBehaviour is wired.
    /// Implements the full IAudioService interface so ServiceLocator stays valid.
    /// </summary>
    public class AudioManager : IAudioService
    {
        private float _musicVolume = 0.6f;
        private float _sfxVolume   = 0.8f;
        private bool  _isMuted     = false;

        private readonly Dictionary<string, AudioClip> _clipCache = new();

        public float MusicVolume => _musicVolume;
        public float SfxVolume   => _sfxVolume;
        public bool  IsMuted     => _isMuted;

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

        public void SetMuted(bool muted)
        {
            _isMuted = muted;
            GameLogger.Debug("AudioManager", $"Muted: {_isMuted}");
        }

        // ── Legacy string-based ───────────────────────────────────────────────
        public void PlaySfx(string clipName)
        {
            GameLogger.Debug("AudioManager", $"PlaySfx: {clipName}");
        }

        public void PlayMusic(string clipName, bool loop = true)
        {
            GameLogger.Debug("AudioManager", $"PlayMusic: {clipName} loop={loop}");
        }

        public void StopMusic()
        {
            GameLogger.Debug("AudioManager", "StopMusic");
        }

        // ── Typed SoundId API ─────────────────────────────────────────────────
        public void Play(SoundId id)
        {
            GameLogger.Debug("AudioManager", $"Play: {id}");
        }

        public void PlayMusic(SoundId id)
        {
            GameLogger.Debug("AudioManager", $"PlayMusic: {id}");
        }

        public void StopMusic(float fadeOut)
        {
            GameLogger.Debug("AudioManager", $"StopMusic fadeOut={fadeOut}");
        }
    }
}
