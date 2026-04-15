namespace VerdantBrews
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;

    /// <summary>
    /// Controls the Pause Menu UI, including pause/resume and quit functionality.
    /// Handles input system events and button callbacks.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the UI document containing the pause button.")]
        private UIDocument uiDocumentPauseButton;

        private UIDocument uiDocument;
        private VisualElement root;

        private Button resumeBtn;
        private Button quitBtn;

        private InputSystem_Actions inputSystem;
        private bool isPaused = false;

        /// <summary>
        /// Initialize pause menu UI, buttons, and input actions.
        /// </summary>
        void OnEnable()
        {
            // Hook pause button click
            var pause = uiDocumentPauseButton.rootVisualElement.Q<Button>("Pause");
            pause.clicked += TogglePause;

            // Setup input system for pause key
            inputSystem = new();
            inputSystem.UI.Enable();
            inputSystem.UI.Pause.performed += OnPause;

            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement.Q<VisualElement>("background");

            // Setup menu buttons
            resumeBtn = root.Q<Button>("resume-btn");
            quitBtn = root.Q<Button>("quit-btn");

            resumeBtn.clicked += OnResumeClicked;
            quitBtn.clicked += OnQuit;

            // Hide pause menu initially
            root.style.display = DisplayStyle.None;
            OnResumeClicked();
        }

        /// <summary>
        /// Remove input system events when disabled.
        /// </summary>
        void OnDisable()
        {
            inputSystem.UI.Pause.performed -= OnPause;
            inputSystem.UI.Disable();
        }

        /// <summary>
        /// Triggered when the pause input action is performed.
        /// </summary>
        private void OnPause(InputAction.CallbackContext ctx)
        {
            Debug.Log("Pause");
            TogglePause();
        }

        /// <summary>
        /// Toggles the paused state and shows/hides the pause menu.
        /// </summary>
        private void TogglePause()
        {
            isPaused = !isPaused;

            root.style.display = isPaused ? DisplayStyle.Flex : DisplayStyle.None;
            Time.timeScale = isPaused ? 0f : 1f; // Freeze or resume game
        }

        /// <summary>
        /// Resume the game from pause.
        /// </summary>
        private void OnResumeClicked()
        {
            isPaused = false;
            root.style.display = DisplayStyle.None;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Quit the current game and return to the main menu.
        /// </summary>
        private void OnQuit()
        {
            Debug.Log("Go to main menu");
            StartCoroutine(LoadMenuAsync());
        }

        /// <summary>
        /// Loads the main menu asynchronously while disabling buttons and switching music.
        /// </summary>
        private IEnumerator LoadMenuAsync()
        {
            // Disable buttons while loading
            resumeBtn.SetEnabled(false);
            quitBtn.SetEnabled(false);

            var load = SceneManager.LoadSceneAsync("MainMenu");
            load.allowSceneActivation = false;

            // Wait until scene is ready (progress 0.9)
            while (load.progress < 0.9f) yield return null;

            // Stop current music and play menu music
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayMusic("Menu");

            load.allowSceneActivation = true;
            while (!load.isDone) yield return null;
        }
    }
}