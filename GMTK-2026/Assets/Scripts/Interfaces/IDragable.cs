using UnityEngine;

namespace Interfaces
{
    public interface IDragable
    {
        public void Pickup();
        // Called every frame while carried, with the pointer's screen position.
        public void HandleDrag(Vector3 screenPos);
        public void Drop(Vector3 pos);
    }
}
