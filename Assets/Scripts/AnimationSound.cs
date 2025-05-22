using UnityEngine;

public class AnimationSound : MonoBehaviour
{
    public AudioClip sound;
    public AudioSource source;

    public void PlaySound()
    {
        source.PlayOneShot(sound);
    }
}