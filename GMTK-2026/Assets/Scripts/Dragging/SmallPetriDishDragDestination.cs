using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dragging
{
    public class SmallPetriDishDragDestination : MonoBehaviour, IDragDestination
    {
        [SerializeField] private Cinemachine.CinemachineVirtualCamera _camera;
        
        public void ExecuteDrag(IDragable dragable)
        {
            Vector3 mousePos = _camera.transform.position;
            mousePos.z = 0f;
            dragable.Drop(mousePos);
        }

        public void ExecuteSpawn(GameObject prefab, Vector3 position)
        {
            
        }
    }
}