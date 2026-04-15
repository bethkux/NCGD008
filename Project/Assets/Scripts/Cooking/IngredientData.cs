namespace VerdantBrews
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Verdant Brews/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public string ingredientName;
        [TextArea] public string description;

        [Header ("Qualities")]
        [Range(0, 2)] public int Warmth;
        [Range(0, 2)] public int Relaxation;
        [Range(0, 2)] public int Sharpness;
        [Range(0, 2)] public int Heaviness;
    }
}