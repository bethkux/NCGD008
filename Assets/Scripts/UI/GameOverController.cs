namespace VerdantBrews
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;

    /// <summary>
    /// Controls the Game Over UI and interactions, including restart and quit functionality.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameOverController : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the TimerUI to get the final score and detect game over.")]
        private TimerUI timerUI;

        private UIDocument uiDocument;
        private VisualElement root;

        private Button resumeBtn;
        private Button quitBtn;
        private Label scoreLabel;

        /// <summary>
        /// Initialize UI elements and button callbacks when enabled.
        /// </summary>
        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement.Q<VisualElement>("background");

            // Buttons and score label
            resumeBtn = root.Q<Button>("resume-btn");
            quitBtn = root.Q<Button>("quit-btn");
            scoreLabel = root.Q<Label>("Score");

            // Assign button callbacks
            resumeBtn.clicked += OnRestart;
            quitBtn.clicked += OnQuit;

            // Show Game Over UI when timer ends
            timerUI.onFinishedTimer += Appear;

            // Hide Game Over UI initially
            root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Remove timer event subscription when disabled.
        /// </summary>
        void OnDisable()
        {
            timerUI.onFinishedTimer -= Appear;
        }

        /// <summary>
        /// Show the Game Over screen and display the final score.
        /// </summary>
        private void Appear()
        {
            scoreLabel.text = timerUI.Score.ToString();
            root.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f; // Pause the game
        }

        /// <summary>
        /// Restart the current scene.
        /// </summary>
        void OnRestart()
        {
            RequestGenerator.Reset();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Initiates quitting to the main menu.
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