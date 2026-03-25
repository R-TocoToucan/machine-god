namespace StellarCommand.Core
{
    using UnityEngine;
    using UnityEngine.Audio;

    public class SettingsManager : MonoSingleton<SettingsManager>
    {
        [SerializeField] private AudioMixer _masterMixer;

        // Defaults
        private const float DefaultVolume = 0.75f;
        private const int DefaultBackgroundFps = 10;
        private const int DefaultFullscreen = 1; // 1 = true

        // Cached values
        private int _backgroundFps;
        private int _savedVSyncCount;

        protected override void OnInitialize()
        {
            // Load cached values from PlayerPrefs (no audio here -- mixer not ready yet)
            _backgroundFps = PlayerPrefs.GetInt(SettingsKeys.BackgroundFps, DefaultBackgroundFps);
            _savedVSyncCount = QualitySettings.vSyncCount;
        }

        private void Start()
        {
            // CRITICAL: Audio Mixer SetFloat MUST be called in Start(), not Awake/OnInitialize
            // The mixer is not active during Awake -- calls silently fail (see Research Pitfall 3)
            ApplyAudioSettings();
            ApplyDisplaySettings();
        }

        // -- Public API --

        public float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        // -- Audio --

        public void SetMasterVolume(float linearVolume)
        {
            _masterMixer.SetFloat("MasterVolume", LinearToDecibel(linearVolume));
            SetFloat(SettingsKeys.MasterVolume, linearVolume);
        }

        public void SetMusicVolume(float linearVolume)
        {
            _masterMixer.SetFloat("MusicVolume", LinearToDecibel(linearVolume));
            SetFloat(SettingsKeys.MusicVolume, linearVolume);
        }

        public void SetSfxVolume(float linearVolume)
        {
            _masterMixer.SetFloat("SfxVolume", LinearToDecibel(linearVolume));
            SetFloat(SettingsKeys.SfxVolume, linearVolume);
        }

        public float GetMasterVolume() => GetFloat(SettingsKeys.MasterVolume, DefaultVolume);
        public float GetMusicVolume() => GetFloat(SettingsKeys.MusicVolume, DefaultVolume);
        public float GetSfxVolume() => GetFloat(SettingsKeys.SfxVolume, DefaultVolume);

        // -- Display --

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            SetInt(SettingsKeys.Fullscreen, isFullscreen ? 1 : 0);
        }

        public void SetResolution(int resolutionIndex)
        {
            var resolutions = Screen.resolutions;
            if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
            {
                var res = resolutions[resolutionIndex];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
                SetInt(SettingsKeys.ResolutionIndex, resolutionIndex);
            }
        }

        // -- Background FPS --

        public void SetBackgroundFps(int fps)
        {
            _backgroundFps = fps;
            SetInt(SettingsKeys.BackgroundFps, fps);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // Restore active settings
                QualitySettings.vSyncCount = _savedVSyncCount;
                Application.targetFrameRate = -1; // unlimited (or vsync-controlled)
            }
            else
            {
                // CRITICAL: vSync MUST be disabled before targetFrameRate takes effect
                // (see Research Pitfall 4)
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _backgroundFps;
            }
        }

        // -- Internal --

        private void ApplyAudioSettings()
        {
            float master = GetFloat(SettingsKeys.MasterVolume, DefaultVolume);
            float music = GetFloat(SettingsKeys.MusicVolume, DefaultVolume);
            float sfx = GetFloat(SettingsKeys.SfxVolume, DefaultVolume);

            _masterMixer.SetFloat("MasterVolume", LinearToDecibel(master));
            _masterMixer.SetFloat("MusicVolume", LinearToDecibel(music));
            _masterMixer.SetFloat("SfxVolume", LinearToDecibel(sfx));
        }

        private void ApplyDisplaySettings()
        {
            bool fullscreen = GetInt(SettingsKeys.Fullscreen, DefaultFullscreen) == 1;
            Screen.fullScreen = fullscreen;

            int resIndex = GetInt(SettingsKeys.ResolutionIndex, -1);
            if (resIndex >= 0)
            {
                SetResolution(resIndex);
            }
        }

        private static float LinearToDecibel(float linear)
        {
            return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        }
    }
}
