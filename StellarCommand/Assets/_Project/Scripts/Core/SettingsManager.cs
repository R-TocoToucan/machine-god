using UnityEngine;

namespace StellarCommand.Core
{
    public class SettingsManager : MonoSingleton<SettingsManager>
    {
        private const string KEY_BGM_VOLUME = "Settings_BGMVolume";
        private const string KEY_SFX_VOLUME = "Settings_SFXVolume";
        private const string KEY_UI_VOLUME = "Settings_UIVolume";
        private const string KEY_FULLSCREEN = "Settings_FullScreen";
        private const string KEY_WALLPAPER_MODE = "Settings_WallpaperMode";

        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private float _uiVolume = 1f;
        private bool _fullScreen = true;
        private bool _wallpaperMode;

        public float BGMVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                AudioManager.Instance.SetBGMVolume(_bgmVolume);
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                AudioManager.Instance.SetSFXVolume(_sfxVolume);
            }
        }

        public float UIVolume
        {
            get => _uiVolume;
            set
            {
                _uiVolume = Mathf.Clamp01(value);
                AudioManager.Instance.SetUIVolume(_uiVolume);
            }
        }

        public bool FullScreen
        {
            get => _fullScreen;
            set
            {
                _fullScreen = value;
                Screen.fullScreen = _fullScreen;
            }
        }

        public bool WallpaperMode
        {
            get => _wallpaperMode;
            set
            {
                _wallpaperMode = value;
                // TODO: Implement Win32 hook to embed window behind desktop icons
                // Use user32.dll FindWindow/SetParent to parent under Progman/WorkerW
            }
        }

        private void Start()
        {
            Load();
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(KEY_BGM_VOLUME, _bgmVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, _sfxVolume);
            PlayerPrefs.SetFloat(KEY_UI_VOLUME, _uiVolume);
            PlayerPrefs.SetInt(KEY_FULLSCREEN, _fullScreen ? 1 : 0);
            PlayerPrefs.SetInt(KEY_WALLPAPER_MODE, _wallpaperMode ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            _bgmVolume = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
            _uiVolume = PlayerPrefs.GetFloat(KEY_UI_VOLUME, 1f);
            _fullScreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
            _wallpaperMode = PlayerPrefs.GetInt(KEY_WALLPAPER_MODE, 0) == 1;

            AudioManager.Instance.SetBGMVolume(_bgmVolume);
            AudioManager.Instance.SetSFXVolume(_sfxVolume);
            AudioManager.Instance.SetUIVolume(_uiVolume);
            Screen.fullScreen = _fullScreen;
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}
