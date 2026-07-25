using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dragging
{
    public class BigPetriDishDragDestination : MonoBehaviour, IDragDestination
    {
        public void ExecuteDrag(IDragable dragable)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            dragable.Drop(worldPos);
        }
    }
}