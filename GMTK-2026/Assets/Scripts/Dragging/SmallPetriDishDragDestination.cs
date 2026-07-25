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
            dragable.Drop(_camera.transform.position);
        }
    }
}