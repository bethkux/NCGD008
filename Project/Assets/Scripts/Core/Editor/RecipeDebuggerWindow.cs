namespace VerdantBrews
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class RecipeDebuggerWindow : EditorWindow
    {
        public enum MatchMode
        {
            ExactValue,
            Category
        }

        public enum QualityCategory
        {
            Any,
            Low,
            Medium,
            High
        }

        private Vector2 scrollPosition;

        private MatchMode matchMode = MatchMode.ExactValue;

        private QualityCategory warmthCategory;
        private QualityCategory relaxationCategory;
        private QualityCategory sharpnessCategory;
        private QualityCategory heavinessCategory;


        private UIDocument uiDocument;

        private int targetWarmth;
        private int targetRelaxation;
        private int targetSharpness;
        private int targetHeaviness;

        private List<string> results = new();

        [MenuItem("Tools/Ingredient Combination Debugger")]

        public static void ShowWindow()
        {
            GetWindow<RecipeDebuggerWindow>("Ingredient Debugger");
        }

        private void OnGUI()
        {
            GUILayout.Label("Source", EditorStyles.boldLabel);

            uiDocument = (UIDocument)EditorGUILayout.ObjectField(
                "UIDocument",
                uiDocument,
                typeof(UIDocument),
                true
            );

            GUILayout.Space(10);

            GUILayout.Label("Target Qualities", EditorStyles.boldLabel);

            matchMode = (MatchMode)EditorGUILayout.EnumPopup("Mode", matchMode);

            if (matchMode == MatchMode.ExactValue)
            {
                targetWarmth = EditorGUILayout.IntField("Warmth", targetWarmth);
                targetRelaxation = EditorGUILayout.IntField("Relaxation", targetRelaxation);
                targetSharpness = EditorGUILayout.IntField("Sharpness", targetSharpness);
                targetHeaviness = EditorGUILayout.IntField("Heaviness", targetHeaviness);
            }
            else
            {
                warmthCategory = (QualityCategory)EditorGUILayout.EnumPopup("Warmth", warmthCategory);
                relaxationCategory = (QualityCategory)EditorGUILayout.EnumPopup("Relaxation", relaxationCategory);
                sharpnessCategory = (QualityCategory)EditorGUILayout.EnumPopup("Sharpness", sharpnessCategory);
                heavinessCategory = (QualityCategory)EditorGUILayout.EnumPopup("Heaviness", heavinessCategory);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Combinations"))
            {
                Generate();
            }

            GUILayout.Space(10);

            GUILayout.Label($"Results ({results.Count})", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

            foreach (var r in results)
            {
                EditorGUILayout.LabelField(r);
            }

            EditorGUILayout.EndScrollView();
        }

        private void Generate()
        {
            results.Clear();

            if (uiDocument == null)
            {
                results.Add("UIDocument not assigned.");
                return;
            }

            var root = uiDocument.rootVisualElement;

            var buttons = root.Query<Button>(className: "Ingedient-btn").ToList();

            List<IngredientData> ingredients = new();

            foreach (var button in buttons)
            {
                var ingredient = button.dataSource as IngredientData;
                if (ingredient != null)
                    ingredients.Add(ingredient);
            }

            if (ingredients.Count == 0)
            {
                results.Add("No ingredients found.");
                return;
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                var a = ingredients[i];

                int w1 = a.Warmth;
                int r1 = a.Relaxation;
                int s1 = a.Sharpness;
                int h1 = a.Heaviness;

                if (matchMode == MatchMode.ExactValue)
                {
                    if (
                        w1 > targetWarmth ||
                        r1 > targetRelaxation ||
                        s1 > targetSharpness ||
                        h1 > targetHeaviness)
                        continue;
                }
                else
                {
                    if (
                        w1 > MaxForCategory(warmthCategory) ||
                        r1 > MaxForCategory(relaxationCategory) ||
                        s1 > MaxForCategory(sharpnessCategory) ||
                        h1 > MaxForCategory(heavinessCategory))
                        continue;
                }

                for (int j = i; j < ingredients.Count; j++)
                {
                    var b = ingredients[j];

                    int w2 = w1 + b.Warmth;
                    int r2 = r1 + b.Relaxation;
                    int s2 = s1 + b.Sharpness;
                    int h2 = h1 + b.Heaviness;

                    if (matchMode == MatchMode.ExactValue)
                    {
                        if (
                            w2 > targetWarmth ||
                            r2 > targetRelaxation ||
                            s2 > targetSharpness ||
                            h2 > targetHeaviness)
                            continue;
                    }
                    else
                    {
                        if (
                            w2 > MaxForCategory(warmthCategory) ||
                            r2 > MaxForCategory(relaxationCategory) ||
                            s2 > MaxForCategory(sharpnessCategory) ||
                            h2 > MaxForCategory(heavinessCategory))
                            continue;
                    }

                    for (int k = j; k < ingredients.Count; k++)
                    {
                        var c = ingredients[k];

                        int w3 = w2 + c.Warmth;
                        int r3 = r2 + c.Relaxation;
                        int s3 = s2 + c.Sharpness;
                        int h3 = h2 + c.Heaviness;

                        bool match;

                        if (matchMode == MatchMode.ExactValue)
                        {
                            match =
                                w3 == targetWarmth &&
                                r3 == targetRelaxation &&
                                s3 == targetSharpness &&
                                h3 == targetHeaviness;
                        }
                        else
                        {
                            match =
                                MatchesCategory(w3, warmthCategory) &&
                                MatchesCategory(r3, relaxationCategory) &&
                                MatchesCategory(s3, sharpnessCategory) &&
                                MatchesCategory(h3, heavinessCategory);
                        }

                        if (match)
                        {
                            results.Add($"{a.name} + {b.name} + {c.name}  ({w3},{r3},{s3},{h3})");
                        }
                    }
                }
            }

            if (results.Count == 0)
                results.Add("No valid combinations found.");
        }

        private bool MatchesCategory(int value, QualityCategory category)
        {
            if (category == QualityCategory.Any)
                return true;

            return category switch
            {
                QualityCategory.Low => value <= 1,
                QualityCategory.Medium => value >= 2 && value <= 3,
                QualityCategory.High => value >= 4,
                _ => false
            };
        }

        private int MaxForCategory(QualityCategory category)
        {
            return category switch
            {
                QualityCategory.Low => 1,
                QualityCategory.Medium => 3,
                QualityCategory.High => 5,
                _ => int.MaxValue
            };
        }
    }
}