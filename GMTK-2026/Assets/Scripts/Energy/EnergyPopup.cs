using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Energy
{
    // One floating payout number: pops in, rises and fades, then hands itself back to its spawner.
    [RequireComponent(typeof(TMP_Text))]
    public class EnergyPopup : MonoBehaviour
    {
        [SerializeField, Tooltip("How far the number rises, in world units.")]
        private float riseDistance = 1.2f;
        [SerializeField, Tooltip("Total lifetime of the number, in seconds.")]
        private float duration = 0.9f;
        [SerializeField, Tooltip("Scale it overshoots to as it pops in, relative to the prefab's scale.")]
        private float popScale = 1.3f;
        [SerializeField, Tooltip("Seconds for each half of the pop (grow, then settle).")]
        private float popDuration = 0.12f;
        [SerializeField, Range(0f, 1f), Tooltip("Fraction of the lifetime spent fully opaque before fading.")]
        private float holdFraction = 0.4f;

        private TMP_Text text;
        private Vector3 baseScale;
        private Sequence sequence;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            baseScale = transform.localScale;
        }

        public void Play(string label, Color color, Vector3 worldPosition, Action<EnergyPopup> onFinished)
        {
            sequence?.Kill();
            gameObject.SetActive(true);

            text.text = label;
            text.color = color;
            transform.position = worldPosition;
            transform.localScale = Vector3.zero;

            sequence = DOTween.Sequence()
                .Append(transform.DOScale(baseScale * popScale, popDuration).SetEase(Ease.OutBack))
                .Append(transform.DOScale(baseScale, popDuration))
                .Insert(0f, transform.DOMoveY(worldPosition.y + riseDistance, duration).SetEase(Ease.OutCubic))
                .Insert(duration * holdFraction,
                    DOTween.To(() => text.alpha, a => text.alpha = a, 0f, duration * (1f - holdFraction)))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onFinished?.Invoke(this);
                })
                .SetLink(gameObject);
        }
    }
}
