using System.Collections.Generic;
using CoreLoop.Interfaces;
using GameStateMachine.States;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Dragging
{
    public class Dragable : MonoBehaviour, IDragable
    {
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Image _uiImage;

        private Camera _mainCamera;
        private DefaultActions _defaultActions;
        private IGameStateMachine _gameStateMachine;
        private DragState.Factory _dragStateFactory;
        private PlaceDraggedState.Factory _placeDraggedStateFactory;
        private bool _isDragging;
        private Transform _originalParent;
        private Canvas _canvas;

        [Inject]
        public void Construct(
            DefaultActions defaultActions,
            IGameStateMachine gameStateMachine,
            DragState.Factory dragStateFactory,
            PlaceDraggedState.Factory placeDraggedStateFactory)
        {
            _defaultActions = defaultActions;
            _gameStateMachine = gameStateMachine;
            _dragStateFactory = dragStateFactory;
            _placeDraggedStateFactory = placeDraggedStateFactory;
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            _canvas = Object.FindAnyObjectByType<Canvas>();
        }

        private void OnEnable()
        {
            if (_defaultActions != null)
            {
                _defaultActions.Level.Drag.performed += OnDragPerformed;
                _defaultActions.Level.PlaceDragged.performed += OnPlacePerformed;
            }
        }

        private void OnDisable()
        {
            if (_defaultActions != null)
            {
                _defaultActions.Level.Drag.performed -= OnDragPerformed;
                _defaultActions.Level.PlaceDragged.performed -= OnPlacePerformed;
            }
        }

        private void Update()
        {
            if (_isDragging)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    HandleDrag(mousePos);
                }
                else
                {
                    Vector3 worldMousePos = _mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
                    HandleDrag(worldMousePos);
                }
            }
        }

        private void OnDragPerformed(InputAction.CallbackContext context)
        {
            if (_isDragging) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f);

            if (hit != null && hit.gameObject == gameObject)
            {
                Pickup();
            }
        }

        private void OnPlacePerformed(InputAction.CallbackContext context)
        {
            if (!_isDragging) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
            
            // First check for IDragDestination
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.1f);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDragDestination>(out var destination))
                {
                    destination.ExecuteDrag(this);
                    return;
                }
            }
        }

        public void Pickup()
        {
            _isDragging = true;
            _collider.enabled = false;
            
            // Sync UI Image size with SpriteRenderer's visual size
            if (_spriteRenderer != null && _uiImage != null)
            {
                _uiImage.sprite = _spriteRenderer.sprite;
                _uiImage.SetNativeSize();
                
                // Calculate world size of the sprite
                Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
                Vector3 worldScale = _spriteRenderer.transform.lossyScale;
                Vector2 worldSize = new Vector2(spriteSize.x * worldScale.x, spriteSize.y * worldScale.y);

                if (_canvas != null)
                {
                    // For ScreenSpaceOverlay, we need to convert world size to screen pixels
                    if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        // Use Camera.orthographicSize for a more robust world-to-screen unit conversion if camera is orthographic
                        float unitsToPixels;
                        if (_mainCamera.orthographic)
                        {
                            unitsToPixels = (Screen.height * 0.5f) / _mainCamera.orthographicSize;
                        }
                        else
                        {
                            // Fallback to point conversion if not orthographic
                            Vector3 screenPos0 = _mainCamera.WorldToScreenPoint(transform.position);
                            Vector3 screenPos1 = _mainCamera.WorldToScreenPoint(transform.position + Vector3.right * worldSize.x + Vector3.up * worldSize.y);
                            _uiImage.rectTransform.sizeDelta = new Vector2(Mathf.Abs(screenPos1.x - screenPos0.x), Mathf.Abs(screenPos1.y - screenPos0.y));
                            goto ParentStep;
                        }
                        
                        _uiImage.rectTransform.sizeDelta = worldSize * unitsToPixels / 2f;
                    }
                    else
                    {
                        _uiImage.rectTransform.sizeDelta = worldSize;
                    }
                }
            }

            ParentStep:
            _spriteRenderer.enabled = false;
            _uiImage.enabled = true;

            _originalParent = transform.parent;
            if (_canvas != null)
            {
                transform.SetParent(_canvas.transform, true);
            }
            
            _gameStateMachine.ChangeState(_placeDraggedStateFactory.Create());
        }

        public void HandleDrag(Vector3 pos)
        {
            transform.position = pos;
        }

        public void Drop(Vector3 pos)
        {
            int petriDishLayer = LayerMask.NameToLayer("PetriDish");
            int layerMask = ~(1 << petriDishLayer);
            
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(layerMask);
            filter.useLayerMask = true;
            
            Collider2D[] results = new Collider2D[1];

            transform.SetParent(_originalParent, true);
            transform.position = pos;
            
            // Force physics update to ensure the collider is at the correct position before checking
            Physics2D.SyncTransforms();

            // Check if there are any collisions if we were to enable the collider at this position
            _collider.enabled = true;
            
            // OverlapCollider might still be finicky with newly enabled colliders
            // Use OverlapPoint as a fallback if it's a small object or just to be sure
            int count = _collider.OverlapCollider(filter, results);
            
            // Sometimes OverlapCollider on a CircleCollider2D doesn't work well if it was just enabled
            // Let's also check with Physics2D.OverlapCircle if it's a circle collider
            if (count == 0 && _collider is CircleCollider2D circle)
            {
                int fallbackCount = Physics2D.OverlapCircle((Vector2)transform.position, circle.radius * transform.lossyScale.x, filter, results);
                
                for (int i = 0; i < fallbackCount; i++)
                {
                    if (results[i] != _collider)
                    {
                        count = 1;
                        break;
                    }
                }
            }
            
            // Debug log to see what's happening
            if (count > 0)
            {
                Debug.Log($"[Dragable] Blocked by: {results[0].name}");
            }

            if (count == 0)
            {
                _isDragging = false;
                _spriteRenderer.enabled = true;
                _uiImage.enabled = false;
                _collider.enabled = true; // Final state for success
                _gameStateMachine.ChangeState(_dragStateFactory.Create());
            }
            else
            {
                _collider.enabled = false;
                if (_canvas != null)
                {
                    transform.SetParent(_canvas.transform, true);
                }
            }
        }
    }
}