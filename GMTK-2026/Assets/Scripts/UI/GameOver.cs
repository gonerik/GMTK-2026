using Energy;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class GameOver : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject panel;
        
        private void OnEnable()
        {
            _signalBus.Subscribe<EnergyService.OnEnergyLostSignal>(OnGameOver);
            restartButton.onClick.AddListener(RestartGame);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<EnergyService.OnEnergyLostSignal>(OnGameOver);
            restartButton.onClick.RemoveListener(RestartGame);
        }

        private void OnGameOver()
        {
            SceneManager.LoadScene("LoseState");
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}