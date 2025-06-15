using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoiceoverManager : MonoBehaviour
{
    public static VoiceoverManager Instance { get; private set; }

    [Header("Audio")]
    public AudioSource audioSource;
    public float defaultVolume = 1f;

    [Header("XR Rig")]
    public GameObject xrRig; // Assign XR Origin or similar

    [Header("Voice Lines")]
    public AudioClip[] pickupClips;
    public AudioClip[] cauldronAddClips;
    public AudioClip[] checkpointClips;

    private Queue<IEnumerator> voiceQueue = new Queue<IEnumerator>();
    private bool isSpeaking = false;
    private IXRMovementBlockable movementBlocker;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (xrRig != null)
            movementBlocker = xrRig.GetComponentInChildren<IXRMovementBlockable>();
    }

    private void Start()
    {
        // no auto-play here
        StartCoroutine(ProcessQueue());
    }

    public void QueueVoice(AudioClip clip, float delayBefore = 0f, float delayAfter = 0f, bool blockMovement = false)
    {
        if (clip == null) return;
        voiceQueue.Enqueue(PlayClipSequence(clip, delayBefore, delayAfter, blockMovement));
    }

    private IEnumerator PlayClipSequence(AudioClip clip, float delayBefore, float delayAfter, bool blockMovement)
    {
        if (delayBefore > 0f)
            yield return new WaitForSeconds(delayBefore);

        if (blockMovement)
            movementBlocker?.EnableMovement(false);

        isSpeaking = true;
        audioSource.clip = clip;
        audioSource.volume = defaultVolume;
        audioSource.Play();

        yield return new WaitForSeconds(clip.length);

        isSpeaking = false;
        if (blockMovement)
            movementBlocker?.EnableMovement(true);

        if (delayAfter > 0f)
            yield return new WaitForSeconds(delayAfter);
    }

    private IEnumerator ProcessQueue()
    {
        while (true)
        {
            while (voiceQueue.Count > 0)
                yield return StartCoroutine(voiceQueue.Dequeue());

            yield return null;
        }
    }

    public bool IsSpeaking() => isSpeaking;

    /// <summary>
    /// True when nothing is playing and the queue is empty.
    /// </summary>
    public bool IsIdle => !isSpeaking && voiceQueue.Count == 0;

    // === EVENT SHORTCUTS ===

    /*public void OnItemPickup() => QueueVoice(GetRandom(pickupClips));
    public void OnIngredientAddedToCauldron() => QueueVoice(GetRandom(cauldronAddClips));
    public void OnCheckpointReached() => QueueVoice(GetRandom(checkpointClips));

    private AudioClip GetRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }*/

    /// <summary>
    /// Immediately stops any playing clip, nukes the queue,
    /// and restarts the queue processor so new clips fire at once.
    /// </summary>
    public void ClearQueue()
    {
        // Stop the currently playing clip + the queue coroutine
        StopAllCoroutines();

        // Nuke any pending clips
        voiceQueue.Clear();

        // Stop the audio immediately
        if (audioSource.isPlaying)
            audioSource.Stop();

        isSpeaking = false;

        // Restart the queue processor
        StartCoroutine(ProcessQueue());
    }

    public void QueueInspectorClip(AudioClip clip)
    {
        QueueVoice(clip);
    }

}
