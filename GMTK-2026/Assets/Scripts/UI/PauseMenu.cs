using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        private readonly float fadeDuration = 0.2f;

        private void Awake()
        {
            FadeOut(0f);
        }

        private void OnEnable()
        {
            UIPauseButton.OnGamePaused += OnGamePaused;
            
        }
        
        private void OnDisable()
        {
            UIPauseButton.OnGamePaused -= OnGamePaused;
        }
        
        private void OnGamePaused(bool isPaused)
        {
            if(isPaused)
                Pause();
            else
                Resume();
        }

        private void Pause()
        {
            FadeIn();
        }

        private void Resume()
        {
            FadeOut();
        }
        
        private void FadeOut()
        {
            fadeGroup.DOFade(0f, fadeDuration).SetUpdate(true)
                .OnComplete(() => {
                    fadeGroup.interactable = false;
                    fadeGroup.blocksRaycasts = false;
                });
        }

        private void FadeOut(float duration)
        {
            fadeGroup.DOFade(0f, duration).SetUpdate(true)
                .OnComplete(() => {
                    fadeGroup.interactable = false;
                    fadeGroup.blocksRaycasts = false;
                });
        }

        private void FadeIn()
        {
            fadeGroup.interactable = true;
            fadeGroup.blocksRaycasts = true;
            fadeGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        }
    }
}
