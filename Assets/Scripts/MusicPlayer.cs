using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> musicTracks;

    private Queue<AudioClip> shuffledQueue;

    void Start()
    {
        if (musicTracks.Count == 0)
        {
            Debug.LogError("No music tracks assigned!");
            return;
        }

        ShuffleTracks();
        PlayNextTrack();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    void ShuffleTracks()
    {
        // Create a new list and shuffle it
        List<AudioClip> tempList = new List<AudioClip>(musicTracks);
        for (int i = 0; i < tempList.Count; i++)
        {
            AudioClip temp = tempList[i];
            int randomIndex = Random.Range(i, tempList.Count);
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // Fill the queue
        shuffledQueue = new Queue<AudioClip>(tempList);
    }

    void PlayNextTrack()
    {
        if (shuffledQueue.Count == 0)
        {
            ShuffleTracks();
        }

        AudioClip nextTrack = shuffledQueue.Dequeue();
        audioSource.clip = nextTrack;
        audioSource.Play();
    }
}
