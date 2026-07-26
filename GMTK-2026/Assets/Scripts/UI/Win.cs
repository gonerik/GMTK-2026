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
        
        public float totalTime = 650000;   // total duration of your timer
        private float elapsedTime = 0f;
        
        void Update()
        {
            elapsedTime += Time.deltaTime;
            elapsedTime = Mathf.Clamp(elapsedTime, 0f, totalTime);
            
            Debug.Log(elapsedTime);
            
            if(elapsedTime>=10)
            {
                OnGameWin();
            }
        }
        
        private void OnEnable()
        {
            //lobalTimer.OnTimerFinished += OnGameWin;
            _signalBus.Subscribe<GlobalTimer.OnLoseSignal>(OnGameWin);
            restartButton.onClick.AddListener(GoToMainMenu);
        }

        private void GoToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDisable()
        {
            //GlobalTimer.OnTimerFinished -= OnGameWin;
            _signalBus.Unsubscribe<GlobalTimer.OnLoseSignal>(OnGameWin);
            restartButton.onClick.RemoveListener(GoToMainMenu);
        }

        private void OnGameWin()
        {
            Debug.Log("Game Win");
            SceneManager.LoadScene("WinState");
        }
        
    }
}