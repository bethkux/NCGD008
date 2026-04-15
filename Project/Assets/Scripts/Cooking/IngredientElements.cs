namespace VerdantBrews
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;


    /// <summary>
    /// Controls ingredient UI interactions and stat previews using UI Toolkit.
    /// This class is responsible only for presentation and local state.
    /// </summary>
    public class IngredientElements : MonoBehaviour
    {
        private static IngredientElements instance;

        [Header("UI")]
        [Tooltip("UIDocument containing ingredient buttons and stat images.")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Stat Sprites")]
        [Tooltip("Sprites representing stat levels (expected range: 0–5).")]
        [SerializeField] private Sprite[] sprites;

        private const int maxIngredientCount = 3;
        private const int maxQualityLevel = 5;

        private Button accept;
        private Button reset;

        private Label first;
        private Label second;
        private Label third;

        // Current (confirmed) stat visuals
        private Image warmthCurrent;
        private Image relaxationCurrent;
        private Image sharpnessCurrent;
        private Image heavinessCurrent;

        // Hover preview stat visuals
        private Image warmthPreview;
        private Image relaxationPreview;
        private Image sharpnessPreview;
        private Image heavinessPreview;

        // Accumulated stat values
        private int warmthTotal;
        private int relaxationTotal;
        private int sharpnessTotal;
        private int heavinessTotal;

        private bool IsMaxIngredientCount => ingredientCount >= maxIngredientCount;

        // Number of selected ingredients
        private int ingredientCount;

        private readonly Dictionary<QualityType, QualityLevel> newestValues = new();

        public System.Action onFinishedPotion;


        public static IngredientElements Instance
        {
            get {
                //if (instance == null)
                //    Debug.Log("Class is null");
                return instance; 
            }
        }

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Binds UI elements and registers button callbacks.
        /// </summary>
        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;

            // Register interactions for all ingredient buttons
            var buttons = root.Query<Button>(className: "Ingedient-btn").ToList();

            ExtractData(buttons);

            foreach (var button in buttons)
            {
                var label = button.Q<Label>();
                label.style.display = DisplayStyle.None;

                button.RegisterCallback<PointerEnterEvent>(_ =>
                {
                    PreviewIngredient(button);
                    label.style.display = DisplayStyle.Flex;
                });

                button.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    ClearPreview();
                    label.style.display = DisplayStyle.None;
                });
                button.clicked += () => OnIngredientClicked(button);
            }

            first = root.Q<Label>("FirstIng");
            second = root.Q<Label>("SecondIng");
            third = root.Q<Label>("ThirdIng");

            // Cache stat images
            warmthCurrent = root.Q<Image>("WarmthCurrent");
            warmthPreview = root.Q<Image>("WarmthPreview");

            relaxationCurrent = root.Q<Image>("RelaxationCurrent");
            relaxationPreview = root.Q<Image>("RelaxationPreview");

            sharpnessCurrent = root.Q<Image>("SharpnessCurrent");
            sharpnessPreview = root.Q<Image>("SharpnessPreview");

            heavinessCurrent = root.Q<Image>("HeavinessCurrent");
            heavinessPreview = root.Q<Image>("HeavinessPreview");

            accept = root.Q<Button>("accept");
            reset = root.Q<Button>("reset");
            accept.SetEnabled(false);

            // Button callbacks
            accept.clicked += AcceptBtn;
            reset.clicked += ResetBtn;

            // Preview hidden by default
            SetPreviewAlpha(0f);
            UpdateCurrentSprites();
        }

        private void ExtractData(List<Button> buttons)
        {
            List<IngredientData> ingredients = new();

            foreach (var button in buttons)
            {
                var ingredient = button.dataSource as IngredientData;

                if (ingredient != null)
                    ingredients.Add(ingredient);
            }

            RequestGenerator.Ingredients = ingredients;
        }

        /// <summary>
        /// Confirms the potion if ingredient limit reached, updates state, and plays sound.
        /// </summary>
        private void AcceptBtn()
        {
            if (IsMaxIngredientCount)
            {
                SetValues();
                onFinishedPotion?.Invoke();
                ResetCounters();
                UpdateCurrentSprites();
                AudioManager.Instance.PlaySound("Glass");
            }
        }

        /// <summary>
        /// Resets the current potion and plays a reset sound.
        /// </summary>
        private void ResetBtn()
        {
            ResetCounters();
            UpdateCurrentSprites();
            AudioManager.Instance.PlaySound("Paper");
        }


        /// <summary>
        /// Applies an ingredient's stats when its button is clicked.
        /// Resets after reaching the ingredient limit.
        /// </summary>
        private void OnIngredientClicked(Button button)
        {
            if (IsMaxIngredientCount)
                return;

            string[] penSounds = { "Pen1", "Pen2" };

            // Pick random clip
            string randomClip = penSounds[Random.Range(0, penSounds.Length)];

            // Random pitch (slight variation)
            AudioManager.Instance.SetPitchToSound(Random.Range(0.9f, 1.1f));
            AudioManager.Instance.PlaySound(randomClip);

            var ingredient = button.dataSource as IngredientData;
            if (ingredient == null) return;

            newestValues.Clear();

            AddIngredientLevel(ingredient);
            SetThreeLabels(button);

            UpdateCurrentSprites();
            ClearPreview();
        }

        /// <summary>
        /// Updates the three ingredient labels based on the current ingredient count.
        /// Enables the accept button when the last ingredient is added.
        /// </summary>
        private void SetThreeLabels(Button button)
        {
            if (ingredientCount == maxIngredientCount - 2)
                first.text = button.text;
            else if (ingredientCount == maxIngredientCount - 1)
                second.text = button.text;
            else if (ingredientCount == maxIngredientCount)
            {
                accept.SetEnabled(true);
                third.text = button.text;
            }
        }

        /// <summary>
        /// Adds the stats of the given ingredient to the current totals and increments the count.
        /// </summary>
        private void AddIngredientLevel(IngredientData ingredient)
        {
            warmthTotal += ingredient.Warmth;
            relaxationTotal += ingredient.Relaxation;
            sharpnessTotal += ingredient.Sharpness;
            heavinessTotal += ingredient.Heaviness;

            ingredientCount++;
        }

        /// <summary>
        /// Stores the computed quality levels of the potion based on ingredient totals.
        /// </summary>
        private void SetValues()
        {
            newestValues.Add(QualityType.Warmth, CheckLevel(warmthTotal));
            newestValues.Add(QualityType.Relaxation, CheckLevel(relaxationTotal));
            newestValues.Add(QualityType.Sharpness, CheckLevel(sharpnessTotal));
            newestValues.Add(QualityType.Density, CheckLevel(heavinessTotal));
        }

        /// <summary>
        /// Converts a numeric value to a corresponding QualityLevel enum.
        /// </summary>
        private QualityLevel CheckLevel(int val)
        {
            if (val <= 1)
            {
                return QualityLevel.Low;
            }
            else if (val <= 3)
            {
                return QualityLevel.Medium;
            }
            else
            {
                return QualityLevel.High;
            }
        }


        /// <summary>
        /// Clears all accumulated stats and ingredient count.
        /// </summary>
        private void ResetCounters()
        {
            warmthTotal = 0;
            relaxationTotal = 0;
            sharpnessTotal = 0;
            heavinessTotal = 0;
            ingredientCount = 0;

            first.text = "";
            second.text = "";
            third.text = "";

            accept.SetEnabled(false);
        }

        /// <summary>
        /// Shows a semi-transparent stat preview for a hovered ingredient.
        /// </summary>
        private void PreviewIngredient(Button button)
        {
            if (IsMaxIngredientCount)
                return;

            var ingredient = button.dataSource as IngredientData;

            if (ingredient == null) return;

            SetPreviewAlpha(0.4f);

            warmthPreview.sprite =
                sprites[Mathf.Clamp(warmthTotal + ingredient.Warmth, 0, maxQualityLevel)];
            relaxationPreview.sprite =
                sprites[Mathf.Clamp(relaxationTotal + ingredient.Relaxation, 0, maxQualityLevel)];
            sharpnessPreview.sprite =
                sprites[Mathf.Clamp(sharpnessTotal + ingredient.Sharpness, 0, maxQualityLevel)];
            heavinessPreview.sprite =
                sprites[Mathf.Clamp(heavinessTotal + ingredient.Heaviness, 0, maxQualityLevel)];
        }

        /// <summary>
        /// Hides preview visuals.
        /// </summary>
        private void ClearPreview()
        {
            SetPreviewAlpha(0f);
        }

        /// <summary>
        /// Sets opacity for all preview images at once.
        /// </summary>
        private void SetPreviewAlpha(float alpha)
        {
            warmthPreview.style.opacity = alpha;
            relaxationPreview.style.opacity = alpha;
            sharpnessPreview.style.opacity = alpha;
            heavinessPreview.style.opacity = alpha;
        }

        /// <summary>
        /// Updates confirmed stat sprites based on current totals.
        /// </summary>
        private void UpdateCurrentSprites()
        {
            warmthCurrent.sprite = sprites[Mathf.Clamp(warmthTotal, 0, maxQualityLevel)];
            relaxationCurrent.sprite = sprites[Mathf.Clamp(relaxationTotal, 0, maxQualityLevel)];
            sharpnessCurrent.sprite = sprites[Mathf.Clamp(sharpnessTotal, 0, maxQualityLevel)];
            heavinessCurrent.sprite = sprites[Mathf.Clamp(heavinessTotal, 0, maxQualityLevel)];
        }

        public Dictionary<QualityType, QualityLevel> GetValues()
        {
            return newestValues;
        }
    }
}