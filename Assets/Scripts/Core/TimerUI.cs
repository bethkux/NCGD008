namespace VerdantBrews
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Handles the timer UI, score display, and visual effects for time warnings.
    /// </summary>
    public class TimerUI : MonoBehaviour
    {
        public static TimerUI Instance { get; private set; }  // Singleton instance

        [SerializeField] private UIDocument uiDocument;
        [SerializeField, Tooltip("Starting time of the game timer in seconds.")]
        private float startTime = 60f;

        [SerializeField, Tooltip("Time threshold (in seconds) when the timer starts pulsing to warn the player.")]
        private float pulseStartTime = 5f;

        private Label timerLabel;
        private Label scoreLabel;
        private float timeRemaining;
        private bool isRunning = true;

        private Tween pulseTween;
        private bool pulseStarted = false;
        private bool gameOver = false;

        public System.Action onFinishedTimer;  // Callback when timer ends

        private int score = 0;
        public int Score => score;

        /// <summary>
        /// Set singleton instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Initialize UI labels and timer on enable.
        /// </summary>
        void OnEnable()
        {
            var root = uiDocument.rootVisualElement;
            timerLabel = root.Q<Label>("TimerLabel");
            scoreLabel = root.Q<Label>("Score");

            timeRemaining = startTime;
            UpdateTimerText();
        }

        /// <summary>
        /// Updates the timer each frame and handles pulse effect and game over.
        /// </summary>
        void Update()
        {
            if (!isRunning)
                return;

            timeRemaining -= Time.deltaTime;  // Decrease timer

            if (timeRemaining <= 0)
            {
                if (!gameOver)
                {
                    gameOver = true;
                    onFinishedTimer?.Invoke();  // Trigger game over
                }

                timeRemaining = 0;
                isRunning = false;
            }

            // Start pulsing when time is low
            if (!pulseStarted && timeRemaining <= pulseStartTime)
            {
                StartPulse();
                pulseStarted = true;
            }

            UpdateTimerText();
        }

        /// <summary>
        /// Starts the continuous pulse animation for low time warning.
        /// </summary>
        private void StartPulse()
        {
            pulseTween = DOTween.To(
                () => timerLabel.resolvedStyle.scale.value.x,
                x => timerLabel.style.scale = new Scale(new Vector3(x, x, 1)),
                1.15f,
                0.5f
            )
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// Updates the timer label text in mm:ss format.
        /// </summary>
        private void UpdateTimerText()
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);

            timerLabel.text = $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Adds time to the timer and updates pulse effect if necessary.
        /// </summary>
        public void AddTime(float seconds)
        {
            timeRemaining += seconds;

            // Stop low-time pulse if time is increased above threshold
            if (timeRemaining > pulseStartTime && pulseStarted)
            {
                pulseStarted = false;
                pulseTween?.Kill();
                timerLabel.style.scale = new Scale(Vector3.one);
            }

            UpdateTimerText();
            PulseTimer();  // Quick pulse to highlight time added
        }

        public void StartTimer()
        {
            isRunning = true;  // Resume timer
        }

        public void StopTimer()
        {
            isRunning = false;  // Pause timer
        }

        /// <summary>
        /// Reset score to zero.
        /// </summary>
        public void ResetScore()
        {
            score = 0;
            UpdateLabel();
        }

        /// <summary>
        /// Increment score and update label.
        /// </summary>
        public void AddScore()
        {
            score++;
            UpdateLabel();
        }

        /// <summary>
        /// Update the score label text.
        /// </summary>
        private void UpdateLabel()
        {
            scoreLabel.text = score.ToString();
        }

        /// <summary>
        /// Plays a small pulse animation on the timer for feedback.
        /// </summary>
        private void PulseTimer()
        {
            timerLabel.style.scale = new Scale(Vector3.one);

            DOTween.To(
                () => timerLabel.resolvedStyle.scale.value.x,
                x => timerLabel.style.scale = new Scale(new Vector3(x, x, 1)),
                1.5f,
                0.4f
            )
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad);
        }
    }
}