using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class VRSubtitles : MonoBehaviour
{
    public TMP_Text subtitleText; // Assign your TextMeshPro component here

    [System.Serializable]
    public struct SubtitleEntry
    {
        public float startTime;  // seconds
        public float endTime;    // seconds
        [TextArea]
        public string text;
    }

    public List<SubtitleEntry> subtitles = new List<SubtitleEntry>();

    private AudioSource audioSource;

    void Awake()
    {
        if (subtitleText != null)
            subtitleText.text = "";
    }

    public void StartSubtitles(AudioSource source)
    {
        audioSource = source;
        StartCoroutine(RunSubtitles());
    }

    private IEnumerator RunSubtitles()
    {
        while (audioSource != null && audioSource.isPlaying)
        {
            float currentTime = audioSource.time;

            string textToShow = "";
            foreach (var entry in subtitles)
            {
                if (currentTime >= entry.startTime && currentTime <= entry.endTime)
                {
                    textToShow = entry.text;
                    break;
                }
            }

            if (subtitleText != null)
                subtitleText.text = textToShow;

            yield return null;
        }

        // Clear subtitles when done
        if (subtitleText != null)
            subtitleText.text = "";
    }
}
