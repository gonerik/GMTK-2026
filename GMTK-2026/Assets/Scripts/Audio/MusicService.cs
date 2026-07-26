using System;
using FMOD.Studio;
using FMODUnity;
using StudioStopMode = FMOD.Studio.STOP_MODE;

namespace Audio
{
    public interface IMusicService
    {
        void Request(MusicId id);
    }

    public sealed class MusicService : IMusicService, IDisposable
    {
        private readonly MusicCatalog catalog;

        private EventInstance current;
        private MusicId? currentId;

        public MusicService(MusicCatalog catalog)
        {
            this.catalog = catalog;
        }
        

        public void Request(MusicId id)
        {
            if (currentId == id && IsPlaying()) return;

            if (current.isValid())
            {
                current.stop(StudioStopMode.ALLOWFADEOUT);
                current.release();
                current = default;
                currentId = null;
            }
            if (!catalog.TryGet(id, out var reference)) return;
            current = RuntimeManager.CreateInstance(reference);
            

            current.start();
            currentId = id;
        }

        private bool IsPlaying()
        {
            if (!current.isValid()) return false;

            return current.getPlaybackState(out var state) == FMOD.RESULT.OK &&
                   (state == PLAYBACK_STATE.PLAYING ||
                    state == PLAYBACK_STATE.STARTING ||
                    state == PLAYBACK_STATE.SUSTAINING);
        }

        public void Dispose()
        {
            StopCurrent();
        }

        private void StopCurrent()
        {
            if (!current.isValid()) return;
            current.stop(StudioStopMode.ALLOWFADEOUT);
            current.release();
            current = default;
            currentId = null;
        }
    }
}
