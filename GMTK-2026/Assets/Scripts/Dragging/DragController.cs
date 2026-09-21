using System;
using System.Collections.Generic;
using CoreLoop.Interfaces;
using GameStateMachine.States;
using Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Dragging
{
    // The single input listener for dragging. Pressing on a cell picks it up, holding carries it, and
    // releasing drops it on the IDragDestination under the cursor - or back where it was picked up if there
    // is none. Dragable only handles how a carried cell looks.
    public class DragController : IInitializable, ITickable, IDisposable
    {
        private const float PickupRadius = 0.5f;
        private const float DestinationRadius = 0.1f;

        private readonly DefaultActions defaultActions;
        private readonly IGameStateMachine gameStateMachine;
        private readonly DragState.Factory dragStateFactory;
        private readonly PlaceDraggedState.Factory placeDraggedStateFactory;
        private readonly Texture2D cursorTexture;

        private Camera mainCamera;

        // Held as the concrete MonoBehaviour so Unity's == null notices a cell destroyed mid-drag.
        // isCarrying keeps "never picked anything up" apart from "what we carried was destroyed".
        private Dragable carried;
        private bool isCarrying;
        private Vector3 pickupPosition;

        [Inject]
        public DragController(
            DefaultActions defaultActions,
            IGameStateMachine gameStateMachine,
            DragState.Factory dragStateFactory,
            PlaceDraggedState.Factory placeDraggedStateFactory,
            [InjectOptional] Texture2D cursorTexture)
        {
            this.defaultActions = defaultActions;
            this.gameStateMachine = gameStateMachine;
            this.dragStateFactory = dragStateFactory;
            this.placeDraggedStateFactory = placeDraggedStateFactory;
            this.cursorTexture = cursorTexture;
        }

        public void Initialize()
        {
            mainCamera = Camera.main;
            defaultActions.Level.Drag.performed += OnDragPerformed;
            defaultActions.Level.PlaceDragged.performed += OnPlacePerformed;
        }

        public void Dispose()
        {
            defaultActions.Level.Drag.performed -= OnDragPerformed;
            defaultActions.Level.PlaceDragged.performed -= OnPlacePerformed;
        }

        public void Tick()
        {
            if (!isCarrying) return;

            if (carried == null)
            {
                EndDrag();
                return;
            }

            carried.HandleDrag(Mouse.current.position.ReadValue());
        }

        private void OnDragPerformed(InputAction.CallbackContext context)
        {
            if (isCarrying) return;

            Dragable dragable = FindNearestDragable(Mouse.current.position.ReadValue());
            if (dragable == null) return;

            // World position, recorded before Pickup reparents the cell under the canvas.
            pickupPosition = dragable.transform.position;
            carried = dragable;
            isCarrying = true;

            dragable.Pickup();
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
            gameStateMachine.ChangeState(placeDraggedStateFactory.Create());
        }

        // Fires when the mouse button is released: PlaceDragged uses a Release Only press interaction,
        // with an initial state check because it is enabled while the button is already held.
        private void OnPlacePerformed(InputAction.CallbackContext context)
        {
            if (!isCarrying) return;

            if (carried != null)
            {
                IDragDestination destination = FindDestination(Mouse.current.position.ReadValue());
                if (destination != null)
                {
                    // Every destination ends by calling Drop on the dragable.
                    destination.ExecuteDrag(carried);
                }
                else
                {
                    // Released over nothing: return the cell to where it was picked up.
                    carried.Drop(pickupPosition);
                }
            }

            EndDrag();
        }

        private void EndDrag()
        {
            carried = null;
            isCarrying = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            // Only hand input back if the drag state is still the live one; never pull the game out of the
            // pause menu because a drag ended underneath it.
            if (gameStateMachine.CurrentState is PlaceDraggedState)
            {
                gameStateMachine.ChangeState(dragStateFactory.Create());
            }
        }

        // Several colliders can sit under the cursor - touching cells, and the dish's own trigger - so take
        // the Dragable closest to the pointer rather than whichever collider the query happens to return.
        private Dragable FindNearestDragable(Vector2 screenPos)
        {
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            Dragable nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider2D hit in Physics2D.OverlapCircleAll(worldPos, PickupRadius))
            {
                if (!hit.TryGetComponent(out Dragable dragable)) continue;

                float distance = ((Vector2)hit.transform.position - worldPos).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = dragable;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private IDragDestination FindDestination(Vector2 screenPos)
        {
            // Check for UI destinations first
            if (EventSystem.current != null)
            {
                var eventData = new PointerEventData(EventSystem.current);
                eventData.position = screenPos;
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<IDragDestination>(out var uiDestination))
                    {
                        return uiDestination;
                    }
                }
            }

            Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

            // Then check for world-space IDragDestination
            foreach (var hit in Physics2D.OverlapCircleAll(worldPos, DestinationRadius))
            {
                if (hit.TryGetComponent<IDragDestination>(out var destination))
                {
                    return destination;
                }
            }

            return null;
        }
    }
}
