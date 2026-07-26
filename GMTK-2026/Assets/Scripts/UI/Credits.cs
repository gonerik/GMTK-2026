using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Credits : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private Button BackButton;
        [SerializeField] private float duration = 10f;
        [SerializeField] private float endY = 1000f;
        [SerializeField] private bool autoStart = true;

        private Vector3 _startPosition;
        private Tween _activeTween;

        private void Awake()
        {
            if (creditsText != null)
            {
                _startPosition = creditsText.rectTransform.localPosition;
            }
        }

        private void OnEnable()
        {
            if (autoStart)
            {
                StartAnimation();
            }
            BackButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            ResetAnimation();
            BackButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        public void StartAnimation()
        {
            if (creditsText == null) return;

            ResetAnimation();
            
            _activeTween = creditsText.rectTransform
                .DOLocalMoveY(endY, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    gameObject.SetActive(false);
                });
        }

        public void ResetAnimation()
        {
            if (_activeTween != null)
            {
                _activeTween.Kill();
                _activeTween = null;
            }

            if (creditsText != null)
            {
                creditsText.rectTransform.localPosition = _startPosition;
            }
        }
    }
}