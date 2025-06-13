using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyTriggerAnimation : MonoBehaviour
{
    public string targetTag = "Key";              // Tag on RustyKey
    public Animation animation;                   // Animation to play
    public GameObject keyToEnable;                // Optional: new key to activate
    public AudioClip soundClip;                   // Optional: sound to play
    public float soundVolume = 1f;                // Sound volume (0–1)

    private bool hasActivated = false;            // Ensures logic runs only once

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated) return;                 // Already triggered once

        if (other != null && other.CompareTag(targetTag))
        {
            hasActivated = true;                  // Prevent future triggers

            PlayAnimation();
            EnableKey();
            DestroyHeldKey(other.gameObject);
        }
    }

    private void PlayAnimation()
    {
        if (animation != null)
        {
            animation.Play();
        }
        else
        {
            Debug.LogWarning("Animation component not assigned.");
        }
    }

    private void EnableKey()
    {
        if (keyToEnable != null)
        {
            keyToEnable.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No keyToEnable assigned.");
        }
    }

    private void PlaySound()
    {
        if (soundClip != null)
        {
            AudioSource.PlayClipAtPoint(soundClip, transform.position, soundVolume);
        }
        else
        {
            Debug.LogWarning("No soundClip assigned.");
        }
    }

    private void DestroyHeldKey(GameObject keyObject)
    {
        keyObject.transform.SetParent(null);
        Destroy(keyObject);
        Debug.Log("RustyKey destroyed.");
    }
}
