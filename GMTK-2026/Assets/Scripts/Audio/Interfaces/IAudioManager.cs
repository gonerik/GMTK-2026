namespace Audio.Interfaces
{
    public interface IAudioManager
    {
        void SetSfxVolume(float volume01);
        void SetMusicVolume(float volume01);
        void SetMasterVolume(float volume01);
        void PauseAll(bool paused);
    }
}