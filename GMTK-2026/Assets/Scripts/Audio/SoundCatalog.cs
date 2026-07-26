using AYellowpaper.SerializedCollections;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public interface ISoundCatalog
    {
        bool TryGet(SoundId id, out EventReference reference);
    }

    [CreateAssetMenu(menuName = "Audio/Sound Catalog", fileName = "SoundCatalog")]
    public class SoundCatalog : ScriptableObject, ISoundCatalog
    {
        [SerializeField]
        private SerializedDictionary<SoundId, EventReference> eventsMap = new();

        public bool TryGet(SoundId id, out EventReference reference)
        {
            return eventsMap.TryGetValue(id, out reference);
        }
    }
}
