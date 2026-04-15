using UnityEngine;

namespace VerdantBrews
{
    /// <summary>
    /// Serializable settings data used to store player preferences.
    /// </summary>
    [System.Serializable]
    public class SettingsData
    {
        [Tooltip("Selected language for the game.")]
        public string language = "English";

        [Tooltip("Master volume (0 = mute, 1 = full volume).")]
        public float masterVolume = 1f;

        [Tooltip("Music volume (0 = mute, 1 = full volume).")]
        public float musicVolume = 1f;

        [Tooltip("Sound effects volume (0 = mute, 1 = full volume).")]
        public float sfxVolume = 1f;

        [Tooltip("Should the game run in fullscreen mode?")]
        public bool fullscreen = true;

        [Tooltip("Speed multiplier for in-game text/dialogue.")]
        public float textSpeed = 1f;
    }

    /// <summary>
    /// Supported game languages.
    /// </summary>
    public enum Languages
    {
        English,
        Czech,
        Slovak
    }
}