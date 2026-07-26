using Audio;
using CoreLoop.Interfaces;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    [RequireComponent(typeof(Canvas))]
    public class MainMenuController : MonoBehaviour
    {
        [Inject] private readonly MusicService musicService;
        //[Inject] private readonly ISceneLoader sceneLoader;
        [SerializeField] private SettingMenu settingMenu;
        [SerializeField] private Credits credits;
        
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [SerializeField] private PlayableDirector director;

        private void OnEnable()
        {
            musicService.PlayMenuMusic();
            startButton.onClick.AddListener(Play);
            settingsButton.onClick.AddListener(Settings);
            creditsButton.onClick.AddListener(Credits);
            quitButton.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(Play);
            settingsButton.onClick.RemoveListener(Settings);
            creditsButton.onClick.RemoveListener(Credits);
            quitButton.onClick.RemoveListener(Quit);
        }

        public void Play()
        { 
            director.Play();
        }
        
        public void Settings()
        {
            settingMenu.gameObject.SetActive(true);
        }
        public void Credits()
        {
            credits.gameObject.SetActive(true);
        }
        
        public void Quit()
        {
            Application.Quit();
        }
    }
}