using Energy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class Win : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject panel;
        
        private void OnEnable()
        {
            _signalBus.Subscribe<GlobalTimer.OnLoseSignal>(OnGameWin);
            restartButton.onClick.AddListener(GoToMainMenu);
        }

        private void GoToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<GlobalTimer.OnLoseSignal>(OnGameWin);
            restartButton.onClick.RemoveListener(GoToMainMenu);
        }

        private void OnGameWin()
        {
            SceneManager.LoadScene("WinState");
        }
    }
}