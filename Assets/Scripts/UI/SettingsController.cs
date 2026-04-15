namespace VerdantBrews
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Audio;
    using UnityEngine.UIElements;
    using UnityEngine.Localization.Settings;

    /// <summary>
    /// Manages the Settings UI, player preferences, audio, fullscreen, text speed, and language.
    /// Hooks into MainMenuController panels and persists settings via PlayerPrefs.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        public SettingsData Data { get; private set; }

        [Header("Audio & Menu References")]
        [Tooltip("Audio mixer used to control music and SFX volumes.")]
        [SerializeField] private AudioMixer audioMixer;

        [Tooltip("Reference to the main menu controller for panel events.")]
        [SerializeField] private MainMenuController mainMenuController;

        // Runtime reference to the settings panel
        private VisualElement settingsPanel;

        // UI elements
        private Slider music;
        private Slider sound;
        private Toggle fullscreen;
        private Slider textSpeed;
        private DropdownField language;

        // Mapping from dropdown label to localization index
        private readonly Dictionary<string, int> languageToLocaleIndex = new()
        {
            { "English", 0 },
            { "Èesky", 1 },
            { "Slovensky", 2 }
        };

        // Prevents multiple simultaneous locale changes
        private bool activeLocalChange = false;

        private void Awake()
        {
            Load();

            if (mainMenuController != null)
            {
                // Subscribe to main menu events
                mainMenuController.OnSettingsPanelReady += Initialize;
                mainMenuController.OnSettingsPanelView += Load;
                mainMenuController.OnSettingsPanelView += ReloadValues;
            }
        }

        private void OnDestroy()
        {
            if (mainMenuController != null)
            {
                mainMenuController.OnSettingsPanelReady -= Initialize;
                mainMenuController.OnSettingsPanelView -= Load;
                mainMenuController.OnSettingsPanelView -= ReloadValues;
            }
        }

        /// <summary>
        /// Initializes the settings panel when ready.
        /// </summary>
        private void Initialize(VisualElement panel)
        {
            Load();
            settingsPanel = panel;
            BindUI();
            ApplyAll();
        }

        /// <summary>
        /// Reloads current settings values into UI elements.
        /// </summary>
        private void ReloadValues()
        {
            music.value = Data.musicVolume;
            sound.value = Data.sfxVolume;
            fullscreen.value = Data.fullscreen;
            textSpeed.value = Data.textSpeed;
            language.value = Data.language;
        }

        /// <summary>
        /// Binds UI elements and registers value change callbacks.
        /// </summary>
        private void BindUI()
        {
            if (settingsPanel == null) return;

            music = settingsPanel.Q<Slider>("music-volume");
            sound = settingsPanel.Q<Slider>("sfx-volume");
            fullscreen = settingsPanel.Q<Toggle>("fullscreen-toggle");
            textSpeed = settingsPanel.Q<Slider>("text-speed");
            language = settingsPanel.Q<DropdownField>("language-dropdown");
            var applyBtn = settingsPanel.Q<Button>("apply-btn");

            // Populate language dropdown
            language.choices = languageToLocaleIndex.Keys.ToList();

            // Initialize values
            ReloadValues();

            // Register callbacks for changes
            music.RegisterValueChangedCallback(e => Data.musicVolume = e.newValue);
            sound.RegisterValueChangedCallback(e => Data.sfxVolume = e.newValue);
            fullscreen.RegisterValueChangedCallback(e => Data.fullscreen = e.newValue);
            textSpeed.RegisterValueChangedCallback(e => Data.textSpeed = e.newValue);
            language.RegisterValueChangedCallback(e => Data.language = e.newValue);

            // Apply button
            applyBtn.clicked += () =>
            {
                ApplyAll();
                Save();
                mainMenuController.OnBack();
            };
        }

        /// <summary>
        /// Applies all settings: audio, fullscreen, language, and future hooks.
        /// </summary>
        public void ApplyAll()
        {
            SetVolume("Music", Data.musicVolume);
            SetVolume("Sound", Data.sfxVolume);

            Screen.fullScreen = Data.fullscreen;

            ChangeLanguage(Data.language);
        }

        /// <summary>
        /// Changes the game language using the localization system.
        /// </summary>
        private void ChangeLanguage(string language)
        {
            if (activeLocalChange) return;

            if (!languageToLocaleIndex.TryGetValue(language, out int index))
            {
                Debug.LogError($"Unknown language: {language}");
                return;
            }

            StartCoroutine(SetLocale(index));
        }

        /// <summary>
        /// Coroutine that sets the selected locale after localization initialization.
        /// </summary>
        private IEnumerator SetLocale(int localeIndex)
        {
            activeLocalChange = true;

            yield return LocalizationSettings.InitializationOperation;

            var locales = LocalizationSettings.AvailableLocales.Locales;

            if (localeIndex < 0 || localeIndex >= locales.Count)
            {
                Debug.LogError("Locale index out of range");
                activeLocalChange = false;
                yield break;
            }

            LocalizationSettings.SelectedLocale = locales[localeIndex];

            activeLocalChange = false;
        }

        /// <summary>
        /// Sets a volume level in decibels for a mixer group.
        /// </summary>
        private void SetVolume(string group, float value)
        {
            audioMixer.SetFloat(
                group,
                Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f
            );
        }

        /// <summary>
        /// Saves all settings to PlayerPrefs.
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetFloat("musicVolume", Data.musicVolume);
            PlayerPrefs.SetFloat("sfxVolume", Data.sfxVolume);
            PlayerPrefs.SetInt("fullscreen", Data.fullscreen ? 1 : 0);
            PlayerPrefs.SetFloat("textSpeed", Data.textSpeed);
            PlayerPrefs.SetString("language", Data.language);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads settings from PlayerPrefs or sets default values.
        /// </summary>
        public void Load()
        {
            Data = new SettingsData
            {
                masterVolume = PlayerPrefs.GetFloat("masterVolume", 0.5f),
                musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.5f),
                sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 0.5f),
                fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1,
                textSpeed = PlayerPrefs.GetFloat("textSpeed", 0.1f),
                language = PlayerPrefs.GetString("language", "English")
            };
        }
    }
}
