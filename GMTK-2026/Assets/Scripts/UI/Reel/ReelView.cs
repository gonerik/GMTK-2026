using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Reel
{
    public enum ReelDirection { Next, Previous }

    /// <summary>
    /// Purely visual: shows the current option's icon in a content slot, covers it with a lock overlay
    /// while the option is locked, and slides it out/in when the option changes. Has no knowledge of
    /// the option list, index, navigation or locking rules — that all lives in ReelSelectorController.
    /// </summary>
    public class ReelView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("RectTransform that holds the icon and lock overlay. This is what slides during a transition.")]
        private RectTransform _contentSlot;

        [SerializeField, Tooltip("Image inside the content slot that shows the current option's icon.")]
        private Image _icon;

        [SerializeField, Tooltip("Shown over the icon while the option is locked (e.g. a dark translucent Image with a lock). Keep it the last child of the content slot so it draws on top.")]
        private GameObject _lockedOverlay;

        [Header("Transition")]
        [SerializeField, Tooltip("How far (local Y, in pixels) the outgoing/incoming icon travels during a switch.")]
        private float _travelDistance = 60f;

        [SerializeField, Tooltip("Duration of the slide animation, in seconds.")]
        private float _transitionDuration = 0.15f;

        [Header("Pulse")]
        [SerializeField, Tooltip("Scale punch played by Pulse(), as a fraction of this view's scale.")]
        private float _pulseStrength = 0.15f;

        [SerializeField, Tooltip("Duration of the pulse, in seconds.")]
        private float _pulseDuration = 0.3f;

        private Coroutine _activeTransition;

        // Lock state the incoming icon takes when a transition swaps it in. SetLocked updates it too, so an
        // unlock that lands mid-slide isn't overwritten by the value the transition started with.
        private bool _pendingLocked;

        /// <summary>Displays an icon with no animation (used for initial setup).</summary>
        public void SetImmediate(Sprite icon, bool locked)
        {
            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
                _activeTransition = null;
            }

            Show(icon, locked);
            _contentSlot.anchoredPosition = Vector2.zero;
        }

        /// <summary>Slides the current icon out and the new one in, from the given direction.</summary>
        public void PlayTransition(Sprite icon, ReelDirection direction, bool locked)
        {
            if (_activeTransition != null)
                StopCoroutine(_activeTransition);

            _pendingLocked = locked;
            _activeTransition = StartCoroutine(TransitionRoutine(icon, direction));
        }

        /// <summary>Locks or unlocks the option on display, including one still sliding in.</summary>
        public void SetLocked(bool locked)
        {
            _pendingLocked = locked;
            SetOverlay(locked);
        }

        /// <summary>A short scale punch, to draw the eye to the reel.</summary>
        public void Pulse()
        {
            transform.DOKill(complete: true);
            transform.DOPunchScale(Vector3.one * _pulseStrength, _pulseDuration).SetUpdate(true).SetLink(gameObject);
        }

        private IEnumerator TransitionRoutine(Sprite icon, ReelDirection direction)
        {
            // "Next" reads as the reel spinning upward, so the old item exits upward (+Y)
            // and the new one enters from below (-Y). "Previous" is the mirror of that.
            float sign = direction == ReelDirection.Next ? 1f : -1f;

            yield return SlideTo(new Vector2(0f, sign * _travelDistance));

            Show(icon, _pendingLocked);
            _contentSlot.anchoredPosition = new Vector2(0f, -sign * _travelDistance);

            yield return SlideTo(Vector2.zero);

            _activeTransition = null;
        }

        private IEnumerator SlideTo(Vector2 target)
        {
            Vector2 start = _contentSlot.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _transitionDuration);
                _contentSlot.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            _contentSlot.anchoredPosition = target;
        }

        private void Show(Sprite icon, bool locked)
        {
            _icon.sprite = icon;
            _pendingLocked = locked;
            SetOverlay(locked);
        }

        private void SetOverlay(bool locked)
        {
            if (_lockedOverlay) _lockedOverlay.SetActive(locked);
        }
    }
}
