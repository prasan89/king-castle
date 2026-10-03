namespace KingSmash.Services
{
    public interface IAudioService
    {
        float MusicVolume { get; }
        float SfxVolume { get; }
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        void PlaySfx(string clipName);
        void PlayMusic(string clipName, bool loop = true);
        void StopMusic();
    }
}
