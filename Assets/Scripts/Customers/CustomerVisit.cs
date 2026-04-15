namespace VerdantBrews
{
    [System.Serializable]
    public class CustomerVisit
    {
        public CustomerData customer;
        public DialogueData dialogue;

        public bool requiresFlag;
        public string requiredFlag;

        public bool setsFlag;
        public string flagToSet;
    }
}