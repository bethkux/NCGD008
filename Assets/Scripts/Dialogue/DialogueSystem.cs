namespace VerdantBrews
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Localization.Settings;
    using UnityEngine.UIElements;

    /// <summary>
    /// Handles running dialogue sequences using a DialogueController.
    /// Supports callbacks when a dialogue sequence finishes.
    /// </summary>
    [RequireComponent(typeof(DialogueController))]
    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private TimerUI timerUI;
        private static DialogueController dialogueController;
        private static UIDocument document;
        private static Label label;
        private static VisualElement root;

        private void Awake()
        {
            dialogueController = GetComponent<DialogueController>();
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement.Q<VisualElement>("dialogue-root");
            var continueHintContainer = document.rootVisualElement.Q<VisualElement>("continue-hint");
            label = continueHintContainer.Q<Label>(); // gets the child Label

        }

        /// <summary>
        /// Starts a dialogue sequence using the provided data.
        /// Invokes the callback when finished.
        /// </summary>
        /// <param name="data">Dialogue lines and settings</param>
        /// <param name="onFinished">Callback after dialogue ends</param>
        public void StartDialogue2(DialogueData data, System.Action onFinished)
        {
            StartCoroutine(RunDialogue2(data, onFinished));
        }

        /// <summary>
        /// Starts a dialogue sequence using the provided data.
        /// Invokes the callback when finished.
        /// </summary>
        public void StartDialogue(System.Action onFinished)
        {
            StartCoroutine(RunDialogue(onFinished));
        }

        /// <summary>
        /// Coroutine that runs each dialogue line sequentially.
        /// </summary>
        private IEnumerator RunDialogue2(DialogueData data, System.Action onFinished)
        {
            foreach (var line in data.lines)
            {
                // Wait until the line has been shown
                yield return dialogueController.ShowLine(line, false, false);
            }

            // Notify that the dialogue sequence is complete
            onFinished?.Invoke();
        }


        /// <summary>
        /// Coroutine that runs each dialogue line sequentially.
        /// </summary>
        private IEnumerator RunDialogue(System.Action onFinished)
        {
            // Hide the main label and show the dialogue UI
            label.style.visibility = Visibility.Hidden;
            root.style.display = DisplayStyle.Flex;

            // Generate a random customer request
            var line = RequestGenerator.GenerateRandomCustomerRequest();

            // Display the generated request line
            yield return dialogueController.ShowLine(line, false, true);

            // Get current request and current ingredient values
            var currentRequest = RequestGenerator.CurrentRequest;
            var currentValues = IngredientElements.Instance.GetValues();
            bool satisfied = true;

            // Check if the player's potion matches the customer's request
            for (int i = 0; i < currentRequest.Count; i++)
            {
                if (currentRequest[i].Item2 != currentValues[currentRequest[i].Item1])
                {
                    satisfied = false;
                    break;
                }
            }

            string key;

            if (satisfied)
            {
                key = "Dialogue.want";
                timerUI.AddTime(20);   // Reward extra time
                timerUI.AddScore();    // Reward points
                RequestGenerator.RegisterSuccess();
            }
            else
            {
                key = "Dialogue.dontwant";
            }

            // Show the dialogue response
            label.style.visibility = Visibility.Visible;
            var entry = LocalizationSettings.StringDatabase.GetTable("DialogueTemplate").GetEntry(key);
            yield return dialogueController.ShowLine(entry.GetLocalizedString(), false, false);

            // Hide the dialogue UI and invoke callback
            root.style.display = DisplayStyle.None;
            onFinished?.Invoke();
        }
    }
}
