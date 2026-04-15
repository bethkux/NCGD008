namespace VerdantBrews
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Verdant Brews/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [TextArea(3, 10)]
        public string[] lines;
    }
}