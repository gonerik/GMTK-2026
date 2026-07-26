using Audio.Interfaces;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class AudioManager : IAudioManager
    {
        private readonly Bus sfxBus;
        private readonly Bus musicBus;
        private readonly Bus masterBus;

        public AudioManager()
        {
            try
            {
                sfxBus = RuntimeManager.GetBus("bus:/SFX");
                musicBus = RuntimeManager.GetBus("bus:/Music");
                masterBus = RuntimeManager.GetBus("bus:/");
            }
            catch
            {
                // ignored
            }
        }

        public void SetMasterVolume(float volume01)
        {
            masterBus.setVolume(Mathf.Clamp01(volume01));
        }

        public void SetSfxVolume(float volume01)
        {
            sfxBus.setVolume(Mathf.Clamp01(volume01));
        }

        public void SetMusicVolume(float volume01)
        {
            musicBus.setVolume(Mathf.Clamp01(volume01));
        }

        public void PauseAll(bool paused)
        {
            masterBus.setPaused(paused);
        }
    }
}