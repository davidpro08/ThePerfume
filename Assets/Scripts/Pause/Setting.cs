using System.Collections.Generic;
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
        private FullScreenMode _screenMode;

        private void Start()
        {
            // Audio
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);

            // Graphics
            InitializeResolutions();
            InitializeQualitySettings();

            fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // Audio Control
        public void SetMasterVolume(float vol) => masterMixer.SetFloat("Master", Mathf.Log10(vol) * 20);
        public void SetBgmVolume(float vol) => masterMixer.SetFloat("BGM", Mathf.Log10(vol) * 20);
        public void SetSfxVolume(float vol) => masterMixer.SetFloat("SFX", Mathf.Log10(vol) * 20);

        // Graphics Control
        private void InitializeResolutions()
        {
            _resolutions = new List<Resolution>();
            resolutionDropdown.ClearOptions();
            _resolutions.AddRange(Screen.resolutions);
            
            var options = new List<string>();
            _resolutionNum = 0;
            for (int i = 0; i < _resolutions.Count; i++)
            {
                options.Add($"{_resolutions[i].width} x {_resolutions[i].height} {_resolutions[i].refreshRateRatio}hz");
                if (_resolutions[i].width == Screen.width && _resolutions[i].height == Screen.height)
                {
                    _resolutionNum = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = _resolutionNum;
            resolutionDropdown.RefreshShownValue();
            
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        public void SetResolution(int index)
        {
            _resolutionNum = index;
            Screen.SetResolution(_resolutions[_resolutionNum].width, _resolutions[_resolutionNum].height, _screenMode);
        }

        private void InitializeQualitySettings()
        {
            qualityDropdown.ClearOptions();
            var qualityOptions = new List<string>();
            foreach (var qualityName in QualitySettings.names)
            {
                qualityOptions.Add(qualityName);
            }
            qualityDropdown.AddOptions(qualityOptions);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
        }
        
        public void SetFullScreen(bool isFull)
        {
            _screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(_resolutions[_resolutionNum].width, _resolutions[_resolutionNum].height, _screenMode);
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }
    }
}