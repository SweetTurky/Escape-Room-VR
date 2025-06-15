using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(XRGrabInteractable))]
public class JarLidController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The XRGrabInteractable on the jar. Lid can only be grabbed when jar is held.")]
    public XRGrabInteractable jarGrabInteractable;
    public GameObject jarParent;

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
        lidGrabInteractable.enabled = false;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_detached) return;
        _detached = true;

        if (jarParent != null)
        {
            Debug.Log($"JarGrabWatcher: Using specified jarParent '{jarParent.name}'.");

            Transform ingredientPoint = jarParent.transform.Find("IngredientPoint");
            if (ingredientPoint != null)
            {
                Debug.Log($"JarGrabWatcher: Found IngredientPoint '{ingredientPoint.name}' with {ingredientPoint.childCount} child(ren).");

                foreach (Transform ing in ingredientPoint)
                {
                    Debug.Log($"JarGrabWatcher: Preparing ingredient '{ing.name}'.");

                    // Enable XRGrabInteractable on ingredient BEFORE deparenting
                    var ingredientGrab = ing.GetComponent<XRGrabInteractable>();
                    if (ingredientGrab != null)
                    {
                        ingredientGrab.enabled = true;
                        Debug.Log($"JarGrabWatcher: Enabled XRGrabInteractable on '{ing.name}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"JarGrabWatcher: No XRGrabInteractable found on '{ing.name}'.");
                    }

                    // Deparent ingredient
                    ing.SetParent(null, true);
                    Debug.Log($"JarGrabWatcher: '{ing.name}' now detached and parent is '{ing.parent?.name ?? "null"}'.");

                    // Enable physics on the ingredient
                    var rb = ing.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.detectCollisions = true;
                        Debug.Log($"JarGrabWatcher: Physics re-enabled on '{ing.name}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"JarGrabWatcher: No Rigidbody found on '{ing.name}'.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"JarGrabWatcher: 'IngredientPoint' not found under '{jarParent.name}'.");
            }
        }
        else
        {
            Debug.LogWarning("JarGrabWatcher: jarParent reference not assigned.");
        }

        // Now detach the lid
        var joint = GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            Destroy(joint);
            Debug.Log("JarGrabWatcher: ConfigurableJoint destroyed on lid.");
        }

        transform.SetParent(null, true);
        Debug.Log("JarGrabWatcher: Lid unparented.");

        if (detachSound != null)
        {
            _audioSource.PlayOneShot(detachSound);
            Debug.Log("JarGrabWatcher: Played lid detach sound.");
        }

        if (lidGrabInteractable != null)
        {
            lidGrabInteractable.enabled = true;
            Debug.Log("JarGrabWatcher: Lid remains grabbable after detachment.");
        }
    }
}
