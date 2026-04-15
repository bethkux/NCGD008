namespace VerdantBrews
{
    using UnityEngine;

    public class FlameFlicker : MonoBehaviour
    {
        [SerializeField, Tooltip("Base intensity of the light.")]
        private float baseIntensity = 1f;

        [SerializeField, Tooltip("Maximum additional intensity caused by flicker.")]
        private float flickerStrength = 0.5f;

        [SerializeField, Tooltip("Speed at which the light flickers.")]
        private float flickerSpeed = 1f;

        private Light lightSource;
        private float noiseSeed;

        private void Awake()
        {
            lightSource = GetComponent<Light>();

            noiseSeed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseSeed);

            lightSource.intensity = baseIntensity + noise * flickerStrength;
        }
    }
}
