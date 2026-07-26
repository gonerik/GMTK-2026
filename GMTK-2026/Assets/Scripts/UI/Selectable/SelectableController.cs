using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Cell.Selectable
{
    public class SelectableController
    {
        private DefaultActions defaultActions;
        
        public event System.Action<SelectionInfo> OnSelected;
        
        [Inject]
        public SelectableController(DefaultActions defaultActions)
        {
            this.defaultActions = defaultActions;
            defaultActions.Level.Drag.performed += OnDragPerformed;
        }

        private void OnDragPerformed(InputAction.CallbackContext context)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f);

            if (hit != null && hit.TryGetComponent<ISelectable>(out var selectable))
            {
                OnSelected?.Invoke(selectable.GetSelectionInfo());
            }
        }
    }
}