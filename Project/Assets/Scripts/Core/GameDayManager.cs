namespace VerdantBrews
{
    using DG.Tweening;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Manages the game day flow, customer visits, and dialogues.
    /// Supports Story mode (predefined days) and Endless mode (random customers).
    /// </summary>
    public class GameDayManager : MonoBehaviour
    {
        [Header("Dialogue & Game Mode")]
        [Tooltip("Reference to the dialogue system.")]
        [SerializeField] private DialogueSystem DialogueSystem;

        [Tooltip("Select Story mode or Endless mode.")]
        [SerializeField] private Mode GameMode;

        [Tooltip("Dialogue data for random customer visits in Endless mode.")]
        [SerializeField] private DialogueData dialogueData;

        [Header("Days & Customers")]
        [Tooltip("All Story mode days and their visits.")]
        [SerializeField] private List<DaySchedule> allDays;

        [Tooltip("List of possible customers for Endless mode.")]
        [SerializeField] private List<CustomerData> customerData = new();

        [Header("Customer Visuals & Movement")]
        [Tooltip("SpriteRenderer used for displaying the customer.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Starting X position of the customer.")]
        [SerializeField] private float startPosition;

        private float endPosition;

        [Tooltip("Speed of customer movement.")]
        [SerializeField, Range(0.2f, 1.2f)] private float movementSpeed;

        // Tracks whether a dialogue is currently active
        private bool activeDialogue = false;

        [Header("Customer position")]
        public Camera worldCamera;

        [Range(0f, 1f), Tooltip("Percantage of the position of a customer on the screen.")]
        public float endPosScreenXPercent = 0.5f; // 0 = left edge, 1 = right edge


        /// <summary>
        /// Available game modes.
        /// </summary>
        public enum Mode
        {
            Story,
            Endless
        }

        private void Start()
        {
            switch (GameMode)
            {
                case Mode.Story:
                    // Run all days sequentially
                    for (int i = 0; i < allDays.Count; i++)
                        StartDay(i);
                    break;

                case Mode.Endless:
                    // Endless mode handled in Update
                    break;
            }

            endPosition = UpdateXPosition();
        }


        private void Update()
        {
            if (GameMode == Mode.Endless && !activeDialogue)
                GetRandomCustomer();
        }

        /// <summary>
        /// Starts a story day and all its visits.
        /// </summary>
        private void StartDay(int dayIndex)
        {
            for (int i = 0; i < allDays[dayIndex].visits.Count; i++)
                StartNextVisit(dayIndex, i);

            EndDay();
        }

        /// <summary>
        /// Starts a single customer visit in Story mode.
        /// Applies flag requirements and sets flags if needed.
        /// </summary>
        private void StartNextVisit(int dayIndex, int visitIndex)
        {
            var day = allDays[dayIndex];
            var visit = day.visits[visitIndex];

            // Skip visit if required flag is missing
            if (visit.requiresFlag && !GameFlags.Instance.HasFlag(visit.requiredFlag))
                return;

            DialogueSystem.StartDialogue2(visit.dialogue, () =>
            {
                if (visit.setsFlag)
                    GameFlags.Instance.SetFlag(visit.flagToSet);
            });
        }

        /// <summary>
        /// Called at the end of a Story day.
        /// Placeholder for fade-outs, summaries, or transitions.
        /// </summary>
        private void EndDay()
        {
            // TODO: fade out, summary, go to next day
        }

        /// <summary>
        /// Spawns a random customer in Endless mode and handles their entry, dialogue, and exit.
        /// </summary>
        public void GetRandomCustomer2()
        {
            activeDialogue = true;

            // Pick a random customer
            CustomerData customer = customerData[Random.Range(0, customerData.Count)];
            spriteRenderer.sprite = customer.portrait;

            // Move customer to dialogue position
            spriteRenderer.gameObject.transform
                .DOMoveX(endPosition, 1 / movementSpeed)
                .SetEase(Ease.InOutQuart)
                .OnComplete(() =>
                {
                    // Start dialogue
                    DialogueSystem.StartDialogue2(dialogueData, () =>
                    {
                        // Return customer to start
                        spriteRenderer.transform
                            .DOMoveX(startPosition, 1 / movementSpeed)
                            .SetEase(Ease.InOutQuart)
                            .OnComplete(() => activeDialogue = false);
                    });
                });
        }

        /// <summary>
        /// Spawns a random customer in Endless mode and handles their entry, dialogue, and exit.
        /// </summary>
        public void GetRandomCustomer()
        {
            activeDialogue = true;

            // Pick a random customer
            CustomerData customer = customerData[Random.Range(0, customerData.Count)];
            spriteRenderer.sprite = customer.portrait;

            // Move customer to dialogue position
            spriteRenderer.gameObject.transform
                .DOMoveX(endPosition, 1 / movementSpeed)
                .SetEase(Ease.InOutQuart)
                .OnComplete(() =>
                {
                    // Start dialogue
                    DialogueSystem.StartDialogue( () =>
                    {
                        // Return customer to start
                        spriteRenderer.transform
                            .DOMoveX(startPosition, 1 / movementSpeed)
                            .SetEase(Ease.InOutQuart)
                            .OnComplete(() => activeDialogue = false);
                    });
                });
        }

        private float UpdateXPosition()
        {
            Vector3 worldPos = worldCamera.ScreenToWorldPoint(new(Screen.width * endPosScreenXPercent, 0, 0));
            return worldPos.x;
        }
    }
}