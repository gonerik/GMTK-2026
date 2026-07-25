using UnityEngine;

namespace Interfaces
{
    public interface IDragable
    {
        public void Pickup();
        public void HandleDrag(Vector3 pos);
        public void Drop(Vector3 pos);
    }
}