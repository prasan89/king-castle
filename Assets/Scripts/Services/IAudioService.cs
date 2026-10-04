using KingSmash.Audio;

namespace KingSmash.Services
{
    /// <summary>
    /// Unified audio service interface.
    /// Preserves all legacy string-based methods for backwards compatibility
    /// and adds typed SoundId-based API for M12+ systems.
    /// </summary>
    public interface IAudioService
    {
        // ── Volume / mute ─────────────────────────────────────────────────────
        float MusicVolume { get; }
        float SfxVolume   { get; }
        bool  IsMuted     { get; }

        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        void SetMuted(bool muted);

        // ── Legacy string-based API (backwards compat) ────────────────────────
        void PlaySfx(string clipName);
        void PlayMusic(string clipName, bool loop = true);
        void StopMusic();

        // ── Typed SoundId API (M12+) ──────────────────────────────────────────
        void Play(SoundId id);
        void PlayMusic(SoundId id);
        void StopMusic(float fadeOut);
    }
}
