using System.Collections;
using UnityEngine;
using LSL;

namespace LSL4Unity.Samples.RMSInlet
{
    public class RMSInletScaler : MonoBehaviour
    {
        public string StreamName;
        ContinuousResolver resolver;

        double max_chunk_duration = 0.100;

        private StreamInlet inlet;
        private float[,] data_buffer;
        private double[] timestamp_buffer;

        private float maxRMS = float.MinValue;
        private float previousRMS = 0f;

        [Range(0f, 1f)]
        public float smoothing = 0.1f;

        public bool useRotation = false;  //  Toggle between rotation and scale

        [Header("Rotation Settings")]
        public float baseRotationSpeed = 10f; // degrees per second
        public float maxRotationMultiplier = 10f; // Max multiplier for rotation speed

        void Start()
        {
            if (!string.IsNullOrEmpty(StreamName))
            {
                resolver = new ContinuousResolver("name", StreamName);
                StartCoroutine(ResolveExpectedStream());
            }
            else
            {
                Debug.LogError("Please assign a StreamName.");
                this.enabled = false;
            }
        }

        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();
            while (results.Length == 0)
            {
                yield return new WaitForSeconds(0.1f);
                results = resolver.results();
            }

            inlet = new StreamInlet(results[0]);
            Debug.Log($"[LSL] Stream resolved: {results[0].name()}");

            int buffer_samples = Mathf.CeilToInt((float)(inlet.info().nominal_srate() * max_chunk_duration));
            int num_channels = inlet.info().channel_count();

            Debug.Log($"[LSL] Stream has {num_channels} channel(s), buffer size = {buffer_samples}");

            if (num_channels < 1)
            {
                Debug.LogError("Stream must have at least one channel.");
                this.enabled = false;
                yield break;
            }

            data_buffer = new float[buffer_samples, num_channels];
            timestamp_buffer = new double[buffer_samples];
        }

        void Update()
        {
            if (inlet == null || data_buffer == null) return;

            int samples_returned = inlet.pull_chunk(data_buffer, timestamp_buffer);

            if (samples_returned > 0)
            {
                float sumSquares = 0f;
                for (int i = 0; i < samples_returned; i++)
                {
                    float value = data_buffer[i, 1]; // EMG Channel
                    sumSquares += value * value;
                }

                float rms = Mathf.Sqrt(sumSquares / samples_returned);
                rms = Mathf.Lerp(previousRMS, rms, smoothing);
                previousRMS = rms;

                if (rms > maxRMS)
                {
                    maxRMS = rms;
                }

                float normalized = (maxRMS > 0f) ? rms / maxRMS : 0f;

                if (useRotation)
                {
                    //  Rotate based on RMS
                    float speed = baseRotationSpeed + (normalized * baseRotationSpeed * maxRotationMultiplier);
                    transform.Rotate(Vector3.right, speed * Time.deltaTime);
                    Debug.Log($"[LSL] RMS: {rms}, RotSpeed: {speed:F2} deg/s");
                }
                else
                {
                    //  Scale based on RMS
                    float visualScale = Mathf.Lerp(0.2f, 3f, normalized);
                    Vector3 newScale = new Vector3(1f, visualScale, 1f);
                    transform.localScale = newScale;
                    Debug.Log($"[LSL] RMS: {rms}, Scale: {newScale.y:F2}");
                }
            }
        }
    }
}
