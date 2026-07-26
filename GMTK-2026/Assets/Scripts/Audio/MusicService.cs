using FMOD.Studio;
using FMODUnity;

namespace Audio
{
    public class MusicService
    {
        public const string MenuMusic = "event:/Menu";
        public const string GameMusic = "event:/Main theme";
        public const string Intro = "event:/Intro Orchestra";

        private EventInstance _currentMusic;

        public void PlayMenuMusic() => ChangeMusicEvent(MenuMusic);
        public void PlayGameMusic() => ChangeMusicEvent(GameMusic);
        public void PlayIntro() => ChangeMusicEvent(Intro);

        public void ChangeMusicEvent(string eventPath)
        {
            if (_currentMusic.isValid())
            {
                _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _currentMusic.release();
            }

            _currentMusic = RuntimeManager.CreateInstance(eventPath);
            _currentMusic.start();
        }
    }
}