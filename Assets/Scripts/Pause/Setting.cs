using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace PausePanel
{
    public class Setting : MonoBehaviour
    {
        [Header("Audio")]
        public AudioMixer masterMixer;
        public Slider masterSlider;
        public Slider bgmSlider;
        public Slider sfxSlider;

        [Header("Graphics")]
        public TMP_Dropdown resolutionDropdown;
        public Toggle fullScreenToggle;
        public TMP_Dropdown qualityDropdown;

        private List<Resolution> _resolutions;
        private int _resolutionNum;

        private void Start()
        {
            // Initialize UI elements with current settings
            InitializeAudioSettings();
            InitializeGraphicsSettings();

            // Add listeners for UI elements
            AddListeners();
        }

        private void InitializeAudioSettings()
        {
            // Get initial volumes from the mixer and set the sliders
            // The values are in dB, so we need to convert them back to linear (0-1)
            masterMixer.GetFloat("Master", out float masterVol);
            masterSlider.value = Mathf.Pow(10, masterVol / 20);

            masterMixer.GetFloat("BGM", out float bgmVol);
            bgmSlider.value = Mathf.Pow(10, bgmVol / 20);

            masterMixer.GetFloat("SFX", out float sfxVol);
            sfxSlider.value = Mathf.Pow(10, sfxVol / 20);
        }

        private void InitializeGraphicsSettings()
        {
            // Quality Settings
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();

            // Fullscreen Toggle
            fullScreenToggle.isOn = Screen.fullScreen;

            // Resolution Dropdown
            InitializeResolutions();
        }

        private void AddListeners()
        {
            // Audio
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);

            // Graphics
            qualityDropdown.onValueChanged.AddListener(SetQuality);
            fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        // Audio Control
        public void SetMasterVolume(float vol) => masterMixer.SetFloat("Master", Mathf.Log10(vol) * 20);
        public void SetBgmVolume(float vol) => masterMixer.SetFloat("BGM", Mathf.Log10(vol) * 20);
        public void SetSfxVolume(float vol) => masterMixer.SetFloat("SFX", Mathf.Log10(vol) * 20);

        // Graphics Control
        private void InitializeResolutions()
        {
            _resolutions = Screen.resolutions.ToList();
            resolutionDropdown.ClearOptions();
            
            var options = new List<string>();
            _resolutionNum = 0;
            for (int i = 0; i < _resolutions.Count; i++)
            {
                options.Add($"{_resolutions[i].width} x {_resolutions[i].height} @ {Mathf.RoundToInt((float)_resolutions[i].refreshRateRatio.value)}hz");
                if (_resolutions[i].width == Screen.currentResolution.width && 
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    _resolutionNum = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = _resolutionNum;
            resolutionDropdown.RefreshShownValue();
        }

        public void SetResolution(int index)
        {
            _resolutionNum = index;
            Resolution resolution = _resolutions[_resolutionNum];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        }
        
        public void SetFullScreen(bool isFull)
        {
            FullScreenMode screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Resolution resolution = _resolutions[_resolutionNum];
            Screen.SetResolution(resolution.width, resolution.height, screenMode);
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }
    }
}