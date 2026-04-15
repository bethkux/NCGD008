namespace VerdantBrews
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;

    /// <summary>
    /// Controls main menu navigation and scene loading using UI Toolkit.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        private UIDocument uiDocument;

        // Root container from the UIDocument
        private VisualElement root;

        [Header("Main Menu")]
        [Tooltip("Root panel containing main menu buttons.")]
        private VisualElement mainPanel;

        private Button playBtn;
        private Button settingsBtn;
        private Button creditsBtn;
        private Button quitBtn;

        [Header("Settings Panel")]
        [Tooltip("UXML asset used to build the settings panel.")]
        [SerializeField] private VisualTreeAsset settingsUXML;
        public VisualElement settingsPanel;
        [Tooltip("Reference to the settings controller for panel events.")]
        [SerializeField] private SettingsController settingsController;

        [Header("Credits Panel")]
        [Tooltip("UXML asset used to build the credits panel.")]
        [SerializeField] private VisualTreeAsset creditsUXML;
        private VisualElement creditsPanel;

        // Events allow other systems to hook into settings lifecycle
        public event Action<VisualElement> OnSettingsPanelReady;
        public event Action OnSettingsPanelView;

        /// <summary>
        /// Caches UI references and registers button callbacks.
        /// </summary>
        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement.Q<VisualElement>("root");

            // Panels
            mainPanel = root.Q<VisualElement>("main-panel");
            settingsPanel = settingsUXML.CloneTree();
            creditsPanel = creditsUXML.CloneTree();

            // Main menu buttons
            playBtn = root.Q<Button>("play-btn");
            settingsBtn = root.Q<Button>("settings-btn");
            creditsBtn = root.Q<Button>("credits-btn");
            quitBtn = root.Q<Button>("quit-btn");

            // Button callbacks
            playBtn.clicked += OnPlay;
            settingsBtn.clicked += OnSettings;
            creditsBtn.clicked += OnCredits;
            quitBtn.clicked += OnQuit;

            // Back buttons inside sub-panels
            settingsPanel.Q<Button>("back-btn").clicked += OnBack;
            creditsPanel.Q<Button>("back-btn").clicked += OnBack;

            // Expose settings panel once it exists
            OnSettingsPanelReady?.Invoke(settingsPanel);
        }

        private void OnDisable()
        {
            // Defensive cleanup to avoid duplicated callbacks
            playBtn.clicked -= OnPlay;
            settingsBtn.clicked -= OnSettings;
            creditsBtn.clicked -= OnCredits;
            quitBtn.clicked -= OnQuit;
        }

        private void Start()
        {
            AudioManager.Instance.PlayMusic("Menu");
            settingsController.ApplyAll();
        }

        /// <summary>
        /// Returns to the main menu panel.
        /// </summary>
        public void OnBack()
        {
            root.Clear();
            root.Add(mainPanel);
        }

        private void OnPlay()
        {
            Debug.Log("Start game");
            RequestGenerator.Reset();
            StartCoroutine(LoadGameplayAsync());
        }

        /// <summary>
        /// Loads the gameplay scene asynchronously and locks menu input while loading.
        /// </summary>
        private IEnumerator LoadGameplayAsync()
        {
            playBtn.SetEnabled(false);
            settingsBtn.SetEnabled(false);
            creditsBtn.SetEnabled(false);
            quitBtn.SetEnabled(false);

            var load = SceneManager.LoadSceneAsync("Gameplay");
            load.allowSceneActivation = false;

            // Unity reports progress up to 0.9f before activation
            while (load.progress < 0.9f)
                yield return null;

            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayMusic("Gameplay");

            load.allowSceneActivation = true;
            while (!load.isDone)
                yield return null;
        }

        private void OnSettings()
        {
            Debug.Log("Open settings");

            root.Clear();
            root.Add(settingsPanel);
            OnSettingsPanelView?.Invoke();
        }

        private void OnCredits()
        {
            Debug.Log("Open credits");

            root.Clear();
            root.Add(creditsPanel);
        }

        private void OnQuit()
        {
            Debug.Log("Quit");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}