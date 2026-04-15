namespace VerdantBrews
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Handles typewriter-style text writing for UI Labels.
    /// Supports multiple concurrent writers.
    /// </summary>
    public class TextWriter : MonoBehaviour
    {
        private static TextWriter instance;
        private List<TextWriterSingle> textWriters = new();
        private static float minPitch = 0.4f;
        private static float maxPitch = 0.7f;


        private void Awake()
        {
            // Singleton pattern
            instance = this;
        }

        /// <summary>
        /// Adds a typewriter writer to a label via static access.
        /// </summary>
        /// <param name="label">UI label to write to</param>
        /// <param name="text">Text to display</param>
        /// <param name="timePerChar">Delay per character</param>
        /// <param name="invisibleChars">Show remaining text invisibly</param>
        /// <param name="removeWriter">Remove existing writer on this label first</param>
        public static TextWriterSingle AddWriter_Static(Label label, string text, float timePerChar, bool invisibleChars, bool removeWriter)
        {
            if (removeWriter)
                instance.RemoveWriter(label);

            return instance.AddWriter(label, text, timePerChar, invisibleChars);
        }

        /// <summary>
        /// Adds a new typewriter writer (instance version).
        /// </summary>
        private TextWriterSingle AddWriter(Label label, string text, float timePerChar, bool invisibleChars)
        {
            TextWriterSingle textWriterSingle = new TextWriterSingle(label, text, timePerChar, invisibleChars);
            textWriters.Add(textWriterSingle);
            return textWriterSingle;
        }

        /// <summary>
        /// Removes a writer for the given label (static access).
        /// </summary>
        private static void RemoveWriter_Static(Label label)
        {
            instance.RemoveWriter(label);
        }

        /// <summary>
        /// Removes any writer targeting the given label.
        /// </summary>
        private void RemoveWriter(Label label)
        {
            for (int i = 0; i < textWriters.Count; i++)
            {
                if (textWriters[i].GetUI() == label)
                {
                    textWriters.RemoveAt(i);
                    i--; // Adjust index after removal
                }
            }
        }

        private void Update()
        {
            // Update all active writers
            for (int i = 0; i < textWriters.Count; i++)
            {
                if (textWriters[i].Update()) // Returns true if finished
                {
                    textWriters.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Represents a single typewriter effect on a Label.
        /// </summary>
        public class TextWriterSingle
        {
            private readonly Label label;
            private readonly string textToWrite;
            private readonly float timePerChar;
            private readonly bool invisibleChars;
            private int charIndex;
            private float timer;

            public TextWriterSingle(Label label, string text, float timePerChar, bool invisibleChars)
            {
                this.label = label;
                this.timePerChar = timePerChar;
                this.invisibleChars = invisibleChars;
                textToWrite = text;
                charIndex = 0;
            }

            /// <summary>
            /// Updates the writer each frame.
            /// Returns true if writing is complete.
            /// </summary>
            public bool Update()
            {
                if (timePerChar == 0f)
                {
                    label.text = textToWrite;
                    return true;
                }

                timer -= Time.deltaTime;
                while (timer < 0 && charIndex < textToWrite.Length)
                {
                    PlayDialogueSound(charIndex);

                    timer += timePerChar;
                    charIndex++;

                    // Clamp to prevent overshoot
                    int displayLength = Mathf.Min(charIndex, textToWrite.Length);
                    string txt = textToWrite.Substring(0, displayLength);

                    if (invisibleChars)
                    {
                        txt += "<color=#00000000>" + textToWrite.Substring(displayLength) + "</color>";
                    }

                    label.text = txt;
                }

                return charIndex >= textToWrite.Length;
            }

            private static void PlayDialogueSound(int numCharacters)
            {
                // Get the current text speed
                float textSpeed = PlayerPrefs.GetFloat("textSpeed", 0.05f);

                // Map text speed to frequency: higher speed -> higher frequency value (less often)
                // We'll clamp so frequency is at least 1
                int frequency = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(6f, 1f, Mathf.InverseLerp(0f, 0.1f, textSpeed))), 1, 6);

                // Play sound every 'frequency' characters
                if (numCharacters % frequency == 0)
                {
                    AudioManager.Instance.StopSound();
                    AudioManager.Instance.SetPitchToSound(Random.Range(minPitch, maxPitch));
                    AudioManager.Instance.PlaySound("Talking", 0.3f);
                }
            }

            /// <summary>
            /// Checks if the writer is still active.
            /// </summary>
            public bool IsActive()
            {
                return charIndex < textToWrite.Length;
            }

            /// <summary>
            /// Immediately completes writing and removes the writer.
            /// </summary>
            public void WriteAllAndDestroy()
            {
                label.text = textToWrite;
                charIndex = textToWrite.Length;
                RemoveWriter_Static(label);
            }

            /// <summary>
            /// Returns the label this writer is affecting.
            /// </summary>
            public Label GetUI()
            {
                return label;
            }
        }
    }
}
