using System.Collections;
using UnityEngine;
using KingSmash.Services;

namespace KingSmash.Audio
{
    /// <summary>
    /// Full MonoBehaviour AudioService for M12.
    /// Manages an AudioSource pool for SFX (8 sources), one Music source, one Ambient source.
    /// Persists volume/mute settings to PlayerPrefs.
    /// Register this in the scene (or via GameBootstrap) as IAudioService to supersede the stub AudioManager.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class AudioService : MonoBehaviour, IAudioService
    {
        // ── PlayerPrefs keys ──────────────────────────────────────────────────
        private const string PrefKeyMusicVol = "audio_music_vol";
        private const string PrefKeySfxVol   = "audio_sfx_vol";
        private const string PrefKeyMuted    = "audio_muted";

        // ── Crossfade duration ────────────────────────────────────────────────
        private const float CrossfadeDuration = 0.5f;

        [Header("Config")]
        [SerializeField] private AudioConfig _audioConfig;

        // ── SFX pool ──────────────────────────────────────────────────────────
        private const int SfxPoolSize = 8;

        private AudioSource   _musicSource;
        private AudioSource   _ambientSource;
        private AudioSource[] _sfxPool;
        private int           _sfxPoolIndex;

        // ── Volume / mute ─────────────────────────────────────────────────────
        private float _musicVolume = 0.7f;
        private float _sfxVolume   = 0.8f;
        private bool  _isMuted     = false;

        // ── Volume property accessors ─────────────────────────────────────────
        public float MusicVolume => _musicVolume;
        public float SfxVolume   => _sfxVolume;
        public bool  IsMuted     => _isMuted;

        // ── Active music tracking ─────────────────────────────────────────────
        private Coroutine _musicFadeCoroutine;
        private SoundId   _currentMusicId;
        private bool      _hasMusicId;

        // ─────────────────────────────────────────────────────────────────────
        #region Unity lifecycle

        private void Awake()
        {
            BuildPool();
            LoadSettings();
        }

        private void OnDestroy()
        {
            SaveSettings();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Pool construction

        private void BuildPool()
        {
            // Music source
            _musicSource          = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop        = true;

            // Ambient source
            _ambientSource          = gameObject.AddComponent<AudioSource>();
            _ambientSource.playOnAwake = false;
            _ambientSource.loop        = true;

            // SFX pool
            _sfxPool = new AudioSource[SfxPoolSize];
            for (int i = 0; i < SfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool[i] = src;
            }
        }

        /// <summary>
        /// Returns the next available SFX AudioSource (round-robin, stealing if all busy).
        /// No allocation — array is pre-warmed.
        /// </summary>
        private AudioSource GetSfxSource()
        {
            // Try to find a free source first
            for (int i = 0; i < SfxPoolSize; i++)
            {
                int idx = (_sfxPoolIndex + i) % SfxPoolSize;
                if (!_sfxPool[idx].isPlaying)
                {
                    _sfxPoolIndex = (idx + 1) % SfxPoolSize;
                    return _sfxPool[idx];
                }
            }

            // All busy — steal round-robin
            var stolen = _sfxPool[_sfxPoolIndex];
            stolen.Stop();
            _sfxPoolIndex = (_sfxPoolIndex + 1) % SfxPoolSize;
            return stolen;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region IAudioService — Typed SoundId API

        public void Play(SoundId id)
        {
            if (_audioConfig == null) return;

            AudioCue cue = _audioConfig.GetCue(id);
            if (cue == null)
            {
                Core.GameLogger.Warning("AudioService", $"No AudioCue found for SoundId: {id}");
                return;
            }

            AudioClip clip = cue.GetRandomClip();
            if (clip == null) return;

            if (cue.category == AudioCategory.Music)
            {
                PlayMusicCue(cue, clip);
                return;
            }

            if (cue.category == AudioCategory.Ambient)
            {
                PlayAmbientCue(cue, clip);
                return;
            }

            // SFX or UI
            if (_isMuted) return;

            float categoryVolume = (cue.category == AudioCategory.UI) ? _sfxVolume : _sfxVolume;

            AudioSource src = GetSfxSource();
            src.clip   = clip;
            src.volume = cue.volume * categoryVolume;
            src.pitch  = cue.GetRandomPitch();
            src.loop   = cue.loop;
            src.Play();
        }

        public void PlayMusic(SoundId id)
        {
            if (_audioConfig == null) return;

            AudioCue cue = _audioConfig.GetCue(id);
            if (cue == null)
            {
                Core.GameLogger.Warning("AudioService", $"No AudioCue found for music SoundId: {id}");
                return;
            }

            AudioClip clip = cue.GetRandomClip();
            if (clip == null) return;

            _currentMusicId = id;
            _hasMusicId     = true;

            PlayMusicCue(cue, clip);
        }

        public void StopMusic(float fadeOut)
        {
            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            if (fadeOut > 0f)
                _musicFadeCoroutine = StartCoroutine(FadeOutMusic(fadeOut));
            else
                _musicSource.Stop();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region IAudioService — Legacy string-based API

        public void PlaySfx(string clipName)
        {
            // Try to parse as SoundId first
            if (System.Enum.TryParse<SoundId>(clipName, true, out var id))
            {
                Play(id);
                return;
            }

            // Fall back to Resources load
            AudioClip clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                Core.GameLogger.Warning("AudioService", $"PlaySfx: clip not found '{clipName}'");
                return;
            }

            if (_isMuted) return;

            AudioSource src = GetSfxSource();
            src.clip   = clip;
            src.volume = _sfxVolume;
            src.pitch  = 1f;
            src.loop   = false;
            src.Play();
        }

        public void PlayMusic(string clipName, bool loop = true)
        {
            // Try SoundId parse first
            if (System.Enum.TryParse<SoundId>(clipName, true, out var id))
            {
                PlayMusic(id);
                return;
            }

            // Fall back to Resources load
            AudioClip clip = Resources.Load<AudioClip>(clipName);
            if (clip == null)
            {
                Core.GameLogger.Warning("AudioService", $"PlayMusic: clip not found '{clipName}'");
                return;
            }

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            _hasMusicId = false;
            _musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip, loop, _musicVolume));
        }

        public void StopMusic()
        {
            StopMusic(0f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region IAudioService — Volume / Mute

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (!_isMuted)
                _musicSource.volume = _musicVolume;
            SaveSettings();
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public void SetMuted(bool muted)
        {
            _isMuted = muted;
            _musicSource.mute   = muted;
            _ambientSource.mute = muted;
            for (int i = 0; i < SfxPoolSize; i++)
                _sfxPool[i].mute = muted;
            SaveSettings();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Internal helpers

        private void PlayMusicCue(AudioCue cue, AudioClip clip)
        {
            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            _musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip, cue.loop, cue.volume * _musicVolume));
        }

        private void PlayAmbientCue(AudioCue cue, AudioClip clip)
        {
            if (_isMuted) return;
            _ambientSource.clip   = clip;
            _ambientSource.volume = cue.volume * _sfxVolume;
            _ambientSource.loop   = cue.loop;
            _ambientSource.Play();
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop, float targetVolume)
        {
            // Fade out current music
            if (_musicSource.isPlaying)
            {
                float startVol = _musicSource.volume;
                float elapsed  = 0f;
                while (elapsed < CrossfadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / CrossfadeDuration);
                    yield return null;
                }
                _musicSource.Stop();
            }

            // Swap clip and fade in
            _musicSource.clip   = newClip;
            _musicSource.loop   = loop;
            _musicSource.volume = 0f;
            if (!_isMuted)
                _musicSource.Play();

            float fadeElapsed = 0f;
            while (fadeElapsed < CrossfadeDuration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, fadeElapsed / CrossfadeDuration);
                yield return null;
            }
            _musicSource.volume = targetVolume;
            _musicFadeCoroutine = null;
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            float startVol = _musicSource.volume;
            float elapsed  = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
                yield return null;
            }
            _musicSource.Stop();
            _musicFadeCoroutine = null;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Settings persistence

        private void LoadSettings()
        {
            _musicVolume = PlayerPrefs.GetFloat(PrefKeyMusicVol, 0.7f);
            _sfxVolume   = PlayerPrefs.GetFloat(PrefKeySfxVol,   0.8f);
            _isMuted     = PlayerPrefs.GetInt(PrefKeyMuted, 0) != 0;

            _musicSource.volume = _isMuted ? 0f : _musicVolume;
            _musicSource.mute   = _isMuted;
            _ambientSource.mute = _isMuted;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(PrefKeyMusicVol, _musicVolume);
            PlayerPrefs.SetFloat(PrefKeySfxVol,   _sfxVolume);
            PlayerPrefs.SetInt(PrefKeyMuted,       _isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
