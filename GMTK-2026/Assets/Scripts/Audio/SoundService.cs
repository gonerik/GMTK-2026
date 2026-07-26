using UnityEngine;

namespace Audio
{
    public interface ISoundService
    {
        void PlayOneShot(SoundId id);
        void PlayOneShot(SoundId id, Vector3 worldPos);
    }

    public class SoundService : ISoundService
    {
        private readonly ISoundCatalog _catalog;

        public SoundService(ISoundCatalog catalog)
        {
            _catalog = catalog;
        }

        public void PlayOneShot(SoundId id)
        {
            if (!_catalog.TryGet(id, out FMODUnity.EventReference reference)) return;
            FMODUnity.RuntimeManager.PlayOneShot(reference);
        }

        public void PlayOneShot(SoundId id, Vector3 worldPos)
        {
            if (!_catalog.TryGet(id, out FMODUnity.EventReference reference)) return;
            FMODUnity.RuntimeManager.PlayOneShot(reference, worldPos);
        }
    }
}