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
    private bool _hasStoppedOnce = false;

    private void Awake()
    {
        if (rootParticleSystem == null)
            rootParticleSystem = GetComponentInChildren<ParticleSystem>();

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Called when a new jar spawns. Re-enables emission on all systems,
    /// plays the start sound, and resets the stop-sound guard.
    /// </summary>
    public void StartParticles()
    {
        if (rootParticleSystem == null) return;

        // --- Re-enable emission on root + all children ---
        var allParticles = rootParticleSystem.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in allParticles)
        {
            var emission = ps.emission;
            emission.enabled = true;
        }

        // Play particles
        rootParticleSystem.Play(true);

        // Reset flag so next StopEmission() will play its sound
        _hasStoppedOnce = false;

        PlaySound(startSound);
    }

    /// <summary>
    /// Stops emission (letting existing particles die out), preserving your original loop,
    /// and plays the stop sound only once per jar.
    /// </summary>
    public void StopEmission()
    {
        if (rootParticleSystem == null) return;

        // — Your original emission-disable block —
        ParticleSystem[] allParticles = rootParticleSystem.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in allParticles)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        // Play stop sound only the first time this is called after StartParticles()
        if (!_hasStoppedOnce)
        {
            PlaySound(stopSound);
            _hasStoppedOnce = true;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}
