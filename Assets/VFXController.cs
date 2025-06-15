using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(AudioSource))]
public class VFXController : MonoBehaviour
{
    [Header("VFX Settings")]
    [Tooltip("The Visual Effect component to control")]
    public VisualEffect vfx;
    [Tooltip("How long (seconds) before stopping the effect")]
    public float duration = 2.5f;

    [Header("Audio Settings")]
    [Tooltip("One-shot clip to play alongside the VFX")]
    public AudioClip oneShotSfx;
    [Tooltip("Volume for the one-shot SFX (0–1)")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private AudioSource _audioSource;

    private void Awake()
    {
        // Auto-assign VFX if not set
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        // Ensure it’s not playing at startup
        if (vfx != null)
            vfx.Stop();

        // Get or add AudioSource
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Plays the VFX and one-shot SFX immediately, then stops the VFX after <see cref="duration"/>.
    /// </summary>
    public void PlayThenStop()
    {
        if (vfx != null)
            vfx.Play();

        if (oneShotSfx != null)
            _audioSource.PlayOneShot(oneShotSfx, sfxVolume);

        StartCoroutine(StopAfterDelay());
    }

    private IEnumerator StopAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        if (vfx != null)
            vfx.Stop();
    }
}
