using System.Collections.Generic;
using CoreLoop.Interfaces;
using Dragging;
using GameStateMachine.States;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace DefaultNamespace.Syringe
{
    public class Syringe : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private float cooldownDuration = 5f;
        
        private Button button;
        private SyringeCooldown _cooldown;
        
        private DefaultActions _defaultActions;
        private IGameStateMachine _gameStateMachine;
        private PlacingBasicCellState.Factory _placingBasicCellStateFactory;
        private DragState.Factory _dragStateFactory;
        private Camera _mainCamera;

        [Inject]
        public void Construct(
            DefaultActions defaultActions,
            IGameStateMachine gameStateMachine,
            PlacingBasicCellState.Factory placingBasicCellStateFactory,
            DragState.Factory dragStateFactory)
        {
            _defaultActions = defaultActions;
            _gameStateMachine = gameStateMachine;
            _placingBasicCellStateFactory = placingBasicCellStateFactory;
            _dragStateFactory = dragStateFactory;
        }
        
        private void Awake()
        {
            button = GetComponent<Button>();
            _cooldown = GetComponentInChildren<SyringeCooldown>();
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
        
        private void OnEnable()
        {
            if (_defaultActions != null)
            {
                _defaultActions.Level.PlaceDefaultCell.performed += OnPlaceDefaultCell;
            }
        }

        private void OnDisable()
        {
            if (_defaultActions != null)
            {
                _defaultActions.Level.PlaceDefaultCell.performed -= OnPlaceDefaultCell;
            }
        }

        private void OnPlaceDefaultCell(InputAction.CallbackContext obj)
        {
            if (_cooldown != null && _cooldown.IsCooldown) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            // Check for UI destinations first
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                eventData.position = mousePos;
                var results = new List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<IDragDestination>(out var uiDestination))
                    {
                        Vector3 spawnPos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
                        uiDestination.ExecuteSpawn(cellPrefab, spawnPos);
                        if (_cooldown != null) _cooldown.StartCooldown(cooldownDuration);
                        _gameStateMachine.ChangeState(_dragStateFactory.Create());
                        return;
                    }
                }
            }

            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
            
            // Then check for world-space IDragDestination
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.1f);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDragDestination>(out var destination))
                {
                    destination.ExecuteSpawn(cellPrefab, worldPos);
                    if (_cooldown != null) _cooldown.StartCooldown(cooldownDuration);
                    _gameStateMachine.ChangeState(_dragStateFactory.Create());
                    return;
                }
            }
        }

        private void OnButtonClick()
        {
            if (_cooldown != null && _cooldown.IsCooldown) return;
            _gameStateMachine.ChangeState(_placingBasicCellStateFactory.Create());
        }
    }
}