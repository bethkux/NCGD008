namespace VerdantBrews
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Verdant Brews/Customer")]
    public class CustomerData : ScriptableObject
    {
        public string customerId;
        public Sprite portrait;
        public string displayName;
        public Color bubbleColor;
    }
}