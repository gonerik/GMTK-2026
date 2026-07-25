using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Cell.Selectable
{
    public class SelectableUI : MonoBehaviour
    {
        [Inject] private DefaultActions defaultActions;
        private void Start()
        {
            defaultActions.Level.Drag.performed += OnDrag;
        }

        private void OnDrag(InputAction.CallbackContext obj)
        {
            
        }
    }
}