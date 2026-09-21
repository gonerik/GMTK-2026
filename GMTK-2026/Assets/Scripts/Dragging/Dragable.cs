using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Dragging
{
    // The presentation half of dragging: swaps the cell's sprite for a canvas image while it is carried and
    // restores it on drop. DragController owns input, picking, destinations and the drag states.
    public class Dragable : MonoBehaviour, IDragable
    {
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Image _uiImage;

        private Camera _mainCamera;
        private Transform _originalParent;
        private Canvas _canvas;

        private const string PickUpSound = "event:/Pick up";
        private const string DropSound = "event:/Put Down";

        private void Awake()
        {
            _mainCamera = Camera.main;
            _canvas = Object.FindAnyObjectByType<Canvas>();
        }

        private void Start()
        {
            _collider = GetComponent<Collider2D>();
            _uiImage = GetComponent<Image>();
        }

        public void Pickup()
        {
            _collider.enabled = false;
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Sync UI Image size with SpriteRenderer's visual size
            if (_spriteRenderer != null && _uiImage != null)
            {
                _uiImage.sprite = _spriteRenderer.sprite;
                _uiImage.color = _spriteRenderer.color;
                _uiImage.SetNativeSize();

                // Calculate world size of the sprite
                Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
                Vector3 worldScale = _spriteRenderer.transform.lossyScale;
                Vector2 worldSize = new Vector2(spriteSize.x, spriteSize.y);

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

                        _uiImage.rectTransform.sizeDelta = worldSize * unitsToPixels;
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
            FMODUnity.RuntimeManager.PlayOneShot(PickUpSound);

            _originalParent = transform.parent;
            if (_canvas != null)
            {
                transform.SetParent(_canvas.transform, true);
            }
        }

        // Follows the pointer. While carried the cell lives under the canvas, so an overlay canvas wants the
        // raw screen position and anything else wants it converted to world space.
        public void HandleDrag(Vector3 screenPos)
        {
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.position = screenPos;
            }
            else
            {
                transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            }
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
            FMODUnity.RuntimeManager.PlayOneShot(DropSound);
            _spriteRenderer.enabled = true;
            _uiImage.enabled = false;
            _collider.enabled = true; // Final state for success
        }
    }
}
