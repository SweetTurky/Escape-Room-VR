using UnityEngine;

public class ParticleEffectController : MonoBehaviour
{
    [Header("Particle Settings")]
    public ParticleSystem rootParticleSystem;

    [Header("Audio Settings")]
    public AudioClip startSound;
    public AudioClip stopSound;
    public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (rootParticleSystem == null)
        {
            rootParticleSystem = GetComponentInChildren<ParticleSystem>();
        }

        // Add an AudioSource if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Starts the full particle system and plays the start sound.
    /// </summary>
    public void StartParticles()
    {
        if (rootParticleSystem != null)
        {
            rootParticleSystem.Play(true);
        }

        PlaySound(startSound);
    }

    /// <summary>
    /// Gradually stops emission from the particle system, and plays the stop sound.
    /// </summary>
    public void StopEmission()
    {
        if (rootParticleSystem == null) return;

        // Stop emission from root and all child systems
        ParticleSystem[] allParticles = rootParticleSystem.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in allParticles)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        PlaySound(stopSound);

        // Optional: stop system fully after delay if you want
        // StartCoroutine(StopAfterDelay(2f));
    }

    /// <summary>
    /// Play a given sound clip at the object's position.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.PlayOneShot(clip, volume);
    }

    // Optional coroutine if you want to fully stop the system later
    /*
    private IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    */
}
