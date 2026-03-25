namespace StellarCommand.Core
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class SettingsPanel : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        [Header("Display")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        [Header("Performance")]
        [SerializeField] private TMP_InputField _backgroundFpsInput;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Panels")]
        [SerializeField] private GameObject _settingsRoot;
        [SerializeField] private GameObject _mainMenuRoot;

        private void Start()
        {
            LoadCurrentValues();
            BindListeners();
        }

        public void Show()
        {
            LoadCurrentValues();
            _settingsRoot.SetActive(true);
            _mainMenuRoot.SetActive(false);
        }

        public void Hide()
        {
            _settingsRoot.SetActive(false);
            _mainMenuRoot.SetActive(true);
        }

        private void LoadCurrentValues()
        {
            var sm = SettingsManager.Instance;

            _masterVolumeSlider.value = sm.GetMasterVolume();
            _musicVolumeSlider.value = sm.GetMusicVolume();
            _sfxVolumeSlider.value = sm.GetSfxVolume();

            _fullscreenToggle.isOn = sm.GetInt(SettingsKeys.Fullscreen, 1) == 1;

            PopulateResolutions();

            int bgFps = sm.GetInt(SettingsKeys.BackgroundFps, 10);
            _backgroundFpsInput.text = bgFps.ToString();
        }

        private void BindListeners()
        {
            _masterVolumeSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetMasterVolume(v));
            _musicVolumeSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetMusicVolume(v));
            _sfxVolumeSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetSfxVolume(v));
            _fullscreenToggle.onValueChanged.AddListener(v => SettingsManager.Instance.SetFullscreen(v));
            _resolutionDropdown.onValueChanged.AddListener(v => SettingsManager.Instance.SetResolution(v));
            _backgroundFpsInput.onEndEdit.AddListener(OnBackgroundFpsChanged);
            _backButton.onClick.AddListener(Hide);
        }

        private void OnBackgroundFpsChanged(string value)
        {
            if (int.TryParse(value, out int fps) && fps >= 1 && fps <= 240)
            {
                SettingsManager.Instance.SetBackgroundFps(fps);
            }
            else
            {
                _backgroundFpsInput.text = "10"; // reset to default
                SettingsManager.Instance.SetBackgroundFps(10);
            }
        }

        private void PopulateResolutions()
        {
            _resolutionDropdown.ClearOptions();
            var resolutions = Screen.resolutions;
            var options = new System.Collections.Generic.List<string>();
            int currentIndex = 0;
            int savedIndex = SettingsManager.Instance.GetInt(SettingsKeys.ResolutionIndex, -1);

            for (int i = 0; i < resolutions.Length; i++)
            {
                var r = resolutions[i];
                options.Add($"{r.width} x {r.height} @ {r.refreshRateRatio.value:F0}Hz");
                if (i == savedIndex) currentIndex = i;
            }

            _resolutionDropdown.AddOptions(options);
            _resolutionDropdown.value = currentIndex;
            _resolutionDropdown.RefreshShownValue();
        }
    }
}
