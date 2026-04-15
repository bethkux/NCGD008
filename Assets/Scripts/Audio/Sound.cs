namespace VerdantBrews
{
    using UnityEngine;

    /// <summary>
    /// Represents a sound with a name and associated audio clip.
    /// Used for music and sound effects in AudioManager.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        [Tooltip("Name identifier for this sound.")]
        public string name;

        [Tooltip("Audio clip to be played for this sound.")]
        public AudioClip clip;
    }
}