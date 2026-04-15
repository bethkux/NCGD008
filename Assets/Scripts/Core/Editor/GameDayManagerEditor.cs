namespace VerdantBrews
{
    using UnityEditor;

    [CustomEditor(typeof(GameDayManager))]
    public class GameDayManagerEditor : Editor
    {
        SerializedProperty dialogueSystem;
        SerializedProperty gameMode;
        SerializedProperty allDays;
        SerializedProperty customerData;
        SerializedProperty spriteRenderer;

        void OnEnable()
        {
            dialogueSystem = serializedObject.FindProperty("DialogueSystem");
            gameMode = serializedObject.FindProperty("GameMode");
            allDays = serializedObject.FindProperty("allDays");
            customerData = serializedObject.FindProperty("customerData");
            spriteRenderer = serializedObject.FindProperty("spriteRenderer");
        }

        //public override void OnInspectorGUI()
        //{
            //serializedObject.Update();

            //EditorGUILayout.PropertyField(dialogueSystem);
            //EditorGUILayout.PropertyField(gameMode);

            //EditorGUILayout.Space();

            //if ((GameDayManager.Mode)gameMode.enumValueIndex == GameDayManager.Mode.Story)
            //{
            //    EditorGUILayout.PropertyField(allDays, true);
            //}
            //else
            //{
            //    EditorGUILayout.PropertyField(customerData, true);
            //    EditorGUILayout.PropertyField(spriteRenderer, true);
            //}

            //serializedObject.ApplyModifiedProperties();
        //}
    }
}