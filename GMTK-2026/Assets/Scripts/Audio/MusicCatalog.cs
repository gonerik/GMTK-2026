using AYellowpaper.SerializedCollections;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(menuName = "Audio/Music Catalog", fileName = "MusicCatalog")]
    public class MusicCatalog : ScriptableObject
    {
        [SerializeField]
        private SerializedDictionary<MusicId, EventReference> eventsMap = new();

        public bool TryGet(MusicId id, out EventReference reference)
        {
            return eventsMap.TryGetValue(id, out reference);
        }
    }
}
