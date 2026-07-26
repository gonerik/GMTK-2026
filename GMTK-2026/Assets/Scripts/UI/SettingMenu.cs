using System.Collections.Generic;
using Audio.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class SettingMenu : MonoBehaviour
    {
        [Inject] private readonly IAudioManager audioManager;
        
        [Header( "Audio" )]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        
        [Header( "Graphics" )]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        
        [SerializeField] private Button backButton;
        
        private const string MasterVolumeKey = "MasterVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string FullscreenKey = "Fullscreen";
        private const string ResolutionWidthKey = "ResolutionWidth";
        private const string ResolutionHeightKey = "ResolutionHeight";

        private List<Resolution> filteredResolutions;

        private void Awake()
        {
            LoadAndApplySettings();
            gameObject.SetActive(false);
        }

        private void LoadAndApplySettings()
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

            masterVolumeSlider.value = masterVolume;
            sfxVolumeSlider.value = sfxVolume;
            musicVolumeSlider.value = musicVolume;
            fullscreenToggle.isOn = isFullscreen;

            audioManager.SetMasterVolume(masterVolume);
            audioManager.SetSfxVolume(sfxVolume);
            audioManager.SetMusicVolume(musicVolume);
            
            Screen.fullScreen = isFullscreen;

            if (PlayerPrefs.HasKey(ResolutionWidthKey) && PlayerPrefs.HasKey(ResolutionHeightKey))
            {
                int width = PlayerPrefs.GetInt(ResolutionWidthKey);
                int height = PlayerPrefs.GetInt(ResolutionHeightKey);
                Screen.SetResolution(width, height, isFullscreen);
            }
        }
        
        private void OnEnable()
        {
            UpdateUI();
            
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            backButton.onClick.AddListener(Back);
        }

        private void UpdateUI()
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            musicVolumeSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            fullscreenToggle.isOn = Screen.fullScreen;
            
            InitializeResolutionDropdown();
        }
        
        private void OnDisable()
        {
            PlayerPrefs.Save();
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggle);
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            backButton.onClick.RemoveListener(Back);
        }
        
        
        
        private void InitializeResolutionDropdown()
        {
            Resolution[] resolutions = Screen.resolutions;
            filteredResolutions = new List<Resolution>();
            resolutionDropdown.ClearOptions();

            int[] basicWidths = { 1280, 1366, 1600, 1920, 2560, 3840 };
            HashSet<string> seenResolutions = new HashSet<string>();

            foreach (var res in resolutions)
            {
                bool isBasicWidth = System.Array.Exists(basicWidths, w => w == res.width);
                bool is16x9 = Mathf.Abs((float)res.width / res.height - 16f / 9f) < 0.01f;

                if (isBasicWidth && is16x9)
                {
                    string key = $"{res.width}x{res.height}";
                    if (!seenResolutions.Contains(key))
                    {
                        filteredResolutions.Add(res);
                        seenResolutions.Add(key);
                    }
                }
            }

            // Ensure current resolution is always available
            string currentKey = $"{Screen.width}x{Screen.height}";
            if (!seenResolutions.Contains(currentKey))
            {
                Resolution currentRes = new Resolution { width = Screen.width, height = Screen.height };
                filteredResolutions.Add(currentRes);
            }

            filteredResolutions.Sort((a, b) => a.width.CompareTo(b.width));

            List<string> options = new List<string>();
            int currentResIndex = 0;

            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                string option = $"{filteredResolutions[i].width} x {filteredResolutions[i].height}";
                options.Add(option);
                if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                {
                    currentResIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResIndex;
            resolutionDropdown.RefreshShownValue();
        }
        
        
        #region UIEvents

        private void OnMasterVolumeChanged(float value)
        {
            audioManager.SetMasterVolume(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
        }
        
        private void OnSfxVolumeChanged(float value)
        {
            audioManager.SetSfxVolume(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            audioManager.SetMusicVolume(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
        }

        private void OnFullscreenToggle(bool value)
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        }

        private void OnResolutionChanged(int value)
        {
            Resolution resolution = filteredResolutions[value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt(ResolutionWidthKey, resolution.width);
            PlayerPrefs.SetInt(ResolutionHeightKey, resolution.height);
        }
        
        private void Back()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
        #endregion
    }
}