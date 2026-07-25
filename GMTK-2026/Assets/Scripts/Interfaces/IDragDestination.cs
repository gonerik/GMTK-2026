using UnityEngine;

namespace Interfaces
{
    public interface IDragDestination
    {
        public void ExecuteDrag(IDragable dragable);
        public void ExecuteSpawn(GameObject prefab, Vector3 position);
    }
}