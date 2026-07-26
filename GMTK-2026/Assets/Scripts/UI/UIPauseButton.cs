using System;
using CoreLoop.Interfaces;
using GameStateMachine.States;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class UIPauseButton : MonoBehaviour
    {
        [Inject] IGameStateMachine gameStateMachine;
        [Inject] InMenuState.Factory inMenuState;
        [Inject] GameLoopState.Factory gameLoopState;

        public static event Action<bool> OnGamePaused;
        private Image buttonImage;
        
        [SerializeField] private Sprite pauseSprite;
        [SerializeField] private Sprite resumeSprite;
        
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private Button resetLevelButton;

        private void Start()
        {
            buttonImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            resetLevelButton.onClick.AddListener(OnResetLevelClicked);
        }

        private void OnResetLevelClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnBackToMenuClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void PauseGame()
        {
            if (gameStateMachine.CurrentState is InMenuState)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }

        private void Pause()
        {
            buttonImage.sprite = resumeSprite; 
            gameStateMachine.ChangeState(inMenuState.Create());
            OnGamePaused?.Invoke(true);
        }

        private void UnPause()
        {
            buttonImage.sprite = pauseSprite; 
            gameStateMachine.ChangeState(gameLoopState.Create());
            OnGamePaused?.Invoke(false);
        }
    }
}
