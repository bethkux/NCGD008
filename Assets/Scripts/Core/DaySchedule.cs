namespace VerdantBrews
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Verdant Brews/Day Schedule")]
    public class DaySchedule : ScriptableObject
    {
        public int dayNumber;

        public List<CustomerVisit> visits = new();
    }
}