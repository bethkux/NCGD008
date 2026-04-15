namespace VerdantBrews
{

    using System;
    using UnityEngine;

    /// <summary>
    /// Manages all audio in the game, including music and sound effects.
    /// Implements a singleton pattern to ensure only one instance exists.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        [Tooltip("List of all background music tracks.")]
        public Sound[] music;

        [Tooltip("List of all sound effects.")]
        public Sound[] sfxSounds;

        [Tooltip("Audio source used to play music.")]
        public AudioSource musicSource;

        [Tooltip("Audio source used to play sound effects.")]
        public AudioSource soundSource;

        /// <summary>
        /// Singleton instance accessor. Creates a new AudioManager if none exists.
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();

                    if (instance == null)
                    {
                        GameObject go = new("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Ensures only one instance exists and persists across scenes.
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
            }
            else
            {
                Destroy(gameObject); // Remove duplicates
            }
        }

        /// <summary>
        /// Plays a music track by name.
        /// </summary>
        public void PlayMusic(string name)
        {
            Sound sound = Array.Find(music, x => x.name == name);

            if (sound != null)
            {
                musicSource.clip = sound.clip;
                musicSource.Play();
            }
            else
            {
                Debug.LogError($"Music {name} was not found");
            }
        }

        /// <summary>
        /// Plays a sound effect by name.
        /// </summary>
        public void PlaySound(string name, float volume = 1f)
        {
            Sound sound = Array.Find(sfxSounds, x => x.name == name);

            if (sound != null)
            {
                soundSource.volume = volume;
                soundSource.clip = sound.clip;
                soundSource.Play();
            }
            else
            {
                Debug.LogError($"Sound {name} was not found");
            }
        }

        /// <summary>
        /// Stops the currently playing sound effect.
        /// </summary>
        public void StopSound()
        {
            soundSource.Stop();
        }

        /// <summary>
        /// Stops the currently playing music track.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Sets the pitch of the sound effects audio source.
        /// </summary>
        /// <param name="val">Pitch value to set.</param>
        public void SetPitchToSound(float val)
        {
            soundSource.pitch = val;
        }
    }
}