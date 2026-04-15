namespace VerdantBrews
{
    using DG.Tweening;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Controls displaying dialogue lines in a UI Label with typewriter effect.
    /// Handles user input to skip typing or advance to the next line.
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("UIDocument containing the dialogue UI.")]
        private UIDocument document;

        [SerializeField] private UIDocument minigame;
        private VisualElement minigameRoot;

        // Active TextWriter for the current line
        private TextWriter.TextWriterSingle textWriterSingle;

        // Dialogue bubble label
        private Label bubble;

        // States
        private bool waitingForInput;
        private bool notdrinkandclick;
        private bool isdrink;
        private bool isTyping;

        private void OnEnable()
        {
            minigameRoot = minigame.rootVisualElement;
            minigameRoot.style.translate = new Translate(new Length(60, LengthUnit.Percent), 0);

            document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;
            bubble = root.Q<Label>("dialogue-txt");

            // Register click callback for skipping or advancing dialogue
            root.RegisterCallback<ClickEvent>(_ => OnClick());
            IngredientElements.Instance.onFinishedPotion += OnFinishDrink;
        }

        private void OnDisable()
        {
            IngredientElements.Instance.onFinishedPotion -= OnFinishDrink;
        }

        /// <summary>
        /// Handles mouse/tap clicks during dialogue.
        /// - If text is typing, skip animation.
        /// - If text is done, advance to next line.
        /// </summary>
        private void OnClick()
        {
            // Skip ongoing typewriter animation
            if (isTyping && textWriterSingle != null && textWriterSingle.IsActive())
            {
                textWriterSingle.WriteAllAndDestroy();
                isTyping = false;
                return;
            }

            // Advance to next line after typing finishes
            if (!notdrinkandclick && !isdrink)
            {
                notdrinkandclick = true;
            }
        }

        public void OnFinishDrink()
        {
            if (waitingForInput)
            {
                waitingForInput = false;
            }
        }


        /// <summary>
        /// Displays a line of dialogue using a typewriter effect.
        /// Waits for the line to finish typing and for user click to continue.
        /// </summary>
        /// <param name="text">The dialogue line</param>
        /// <param name="timed">Optional, not currently used</param>
        public IEnumerator ShowLine(string text, bool timed, bool drink)
        {
            isTyping = true;
            waitingForInput = false;
            notdrinkandclick = false;
            isdrink = drink;

            // Start typewriter effect
            textWriterSingle = TextWriter.AddWriter_Static(
                bubble,
                text,
                PlayerPrefs.GetFloat("textSpeed", 0.1f),
                true, // invisible remaining chars for smooth layout
                true  // remove any previous writer on this label
            );

            // Wait until typing is done (naturally or skipped)
            while (textWriterSingle != null && textWriterSingle.IsActive())
                yield return null;

            isTyping = false;
            waitingForInput = true;



            // Slide into the frame
            if (drink)
                SlideIn();

            // Wait for click to advance
            while (waitingForInput)
            {
                if (notdrinkandclick)
                    break;
                yield return null;
            }

            // Slide out of the frame
            if (drink)
                SlideOut();
        }

        /// <summary>
        /// Animates the minigame UI container sliding into view.
        /// </summary>
        private void SlideIn()
        {
            DOTween.To(
                () => minigameRoot.resolvedStyle.translate.x,
                x => minigameRoot.style.translate = new Translate(x, 0),
                0,
                0.4f
            ).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Animates the minigame UI container sliding out of view.
        /// </summary>
        private void SlideOut()
        {
            DOTween.To(
                () => minigameRoot.resolvedStyle.translate.x,
                x => minigameRoot.style.translate = new Translate(new Length(x, LengthUnit.Percent), 0),
                60,
                0.4f
            ).SetEase(Ease.InCubic);
        }
    }
}
