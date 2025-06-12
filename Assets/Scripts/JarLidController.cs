using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(XRGrabInteractable))]
public class JarLidController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The XRGrabInteractable on the jar. Lid can only be grabbed when jar is held.")]
    public XRGrabInteractable jarGrabInteractable;

    [Tooltip("The lid's own XRGrabInteractable.")]
    public XRGrabInteractable lidGrabInteractable;

    [Header("Mount & Jar")]
    [Tooltip("Where the lid sits on the jar (world-space point)")]
    public Transform jarMountPoint;
    [Tooltip("The jar's Rigidbody, for joint anchoring.")]
    public Rigidbody jarRigidbody;

    [Header("Detach Sound")]
    [Tooltip("Sound played the moment it detaches.")]
    public AudioClip detachSound;

    // internal state
    private bool _detached = false;
    private AudioSource _audioSource;

    void Awake()
    {
        // Disable lid grab until jar is held
        if (lidGrabInteractable != null)
            lidGrabInteractable.enabled = false;

        // Prepare audio source for detach sound
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        if (jarGrabInteractable != null)
        {
            jarGrabInteractable.selectEntered.AddListener(OnJarGrab);
            jarGrabInteractable.selectExited.AddListener(OnJarRelease);
        }
        if (lidGrabInteractable != null)
        {
            lidGrabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    void OnDisable()
    {
        if (jarGrabInteractable != null)
        {
            jarGrabInteractable.selectEntered.RemoveListener(OnJarGrab);
            jarGrabInteractable.selectExited.RemoveListener(OnJarRelease);
        }
        if (lidGrabInteractable != null)
        {
            lidGrabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    void OnJarGrab(SelectEnterEventArgs args)
    {
        // Enable lid grabbing only if still attached
        if (!_detached && lidGrabInteractable != null)
            lidGrabInteractable.enabled = true;
    }

    void OnJarRelease(SelectExitEventArgs args)
    {
        // If the lid is still attached, drop it
        if (!_detached && lidGrabInteractable.isSelected)
        {
            // Check if there are any interactors selecting the lid
            if (lidGrabInteractable.interactorsSelecting.Count > 0)
            {
                lidGrabInteractable.interactionManager.SelectExit(
                    (UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)lidGrabInteractable.interactorsSelecting[0],
                    lidGrabInteractable
                );
            }
        }
        // Don’t touch lidGrabInteractable.enabled at all
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Immediately detach on first grab
        if (!_detached)
        {
            _detached = true;

            // Destroy the joint so lid is free
            var joint = GetComponent<ConfigurableJoint>();
            if (joint != null)
                Destroy(joint);

            // Unparent the lid
            transform.SetParent(null, true);

            // Play the pop-open sound once
            if (detachSound != null)
                _audioSource.PlayOneShot(detachSound);

            // Ensure lid remains grabbable after detachment
            if (lidGrabInteractable != null)
                lidGrabInteractable.enabled = true;
        }
    }
}
