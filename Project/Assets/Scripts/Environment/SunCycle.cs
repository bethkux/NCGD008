namespace VerdantBrews
{
    using UnityEngine;

    [ExecuteAlways]
    public class SunCycle : MonoBehaviour
    {
        [Range(0f, 1f), Tooltip("Normalized time of day (0 = midnight, 0.5 = noon, 1 = next midnight).")]
        public float timeOfDay;

        [Tooltip("Length of a full day in seconds.")]
        public float dayLengthSeconds = 180f;

        [Tooltip("Lighting settings just before sunrise.")]
        public Vector2 rightBeforeSunrise;

        [Tooltip("Lighting settings at sunrise.")]
        public Vector2 sunrise;

        [Tooltip("Lighting settings at noon.")]
        public Vector2 noon;

        [Tooltip("Lighting settings at sunset.")]
        public Vector2 sunset;

        [Tooltip("Lighting settings at midnight.")]
        public Vector2 midnight;

        [Tooltip("Array of candle lights to be affected by flicker.")]
        public Light[] candleLights;

        [Range(1f, 8f), Tooltip("Rate at which candle lights flicker.")]
        public float flickerRate = 4f;

        private void Awake()
        {
#if UNITY_ANDROID
            for (int i = 0; i < 3; i++)
            {
                candleLights[candleLights.Length - 1 - i].gameObject.SetActive(false);
            }
#endif
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                timeOfDay += Time.deltaTime / dayLengthSeconds;
                timeOfDay %= 1f;

            }

            UpdateCandles();

            transform.rotation = Quaternion.Euler(EvaluateRotation(timeOfDay));
        }

        Vector3 EvaluateRotation(float t)
        {
            if (t < 0.2f)
                GetComponent<Light>().intensity = 0;
            else
                GetComponent<Light>().intensity = 1.2f;

            // Midnight -> right before Sunrise
            if (t < 0.2f)
            {
                return Vector3.Lerp(
                    midnight,        // midnight
                    rightBeforeSunrise,     // sunrise
                    t / 0.2f
                );
            }

            // right before -> Sunrise
            if (t < 0.25f)
            {
                return Vector3.Lerp(
                    rightBeforeSunrise,        // midnight
                    sunrise,     // sunrise
                    t / 0.25f
                );
            }

            // Sunrise -> Noon
            if (t < 0.5f)
            {
                GetComponent<Light>().colorTemperature = Mathf.Lerp(3200, 4300, (t - 0.25f) / 0.25f);

                return Vector3.Lerp(
                    sunrise,     // sunrise
                    noon,         // noon
                    (t - 0.25f) / 0.25f
                );
            }

            // Noon -> Sunset
            if (t < 0.75f)
            {
                GetComponent<Light>().colorTemperature = Mathf.Lerp(4300, 3200, (t - 0.5f) / 0.25f);

                return Vector3.Lerp(
                    noon,         // noon
                    sunset,        // sunset
                    (t - 0.5f) / 0.25f
                );
            }

            // Sunset -> Midnight
            return Vector3.Lerp(
                sunset,            // sunset
                midnight,            // midnight
                (t - 0.75f) / 0.25f
            );
        }

        void UpdateCandles()
        {
            float candleTarget = 0f;

            // Define ranges
            float fadeInStart = 0.75f;   // start fading in at sunset
            float fadeInEnd = 0.85f;   // fully on
            float fadeOutStart = 0.25f;  // start fading out at sunrise
            float fadeOutEnd = 0.35f;  // fully off

            if (timeOfDay >= fadeInStart && timeOfDay < fadeInEnd) // Evening fade-in
            {
                float t = (timeOfDay - fadeInStart) / (fadeInEnd - fadeInStart);
                candleTarget = Mathf.SmoothStep(0f, 1f, t);
            }
            else if (timeOfDay >= fadeOutStart && timeOfDay < fadeOutEnd) // Morning fade-out
            {
                float t = (timeOfDay - fadeOutStart) / (fadeOutEnd - fadeOutStart);
                candleTarget = Mathf.SmoothStep(1f, 0f, t);
            }
            else if (timeOfDay >= fadeInEnd || timeOfDay < fadeOutStart) // Full night
            {
                candleTarget = 1f;
            }
            else // Day
            {
                candleTarget = 0f;
            }

            // Apply flicker
            foreach (var candle in candleLights)
            {
                float flicker = 0.4f * Mathf.PerlinNoise(Time.time * Random.value, Random.value);
                candle.intensity = candleTarget * (1f + flicker);
            }
        }
    }
}