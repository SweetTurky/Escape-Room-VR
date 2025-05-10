using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ThumbstickScalerInput : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference rightThumbstickAction; // Assign to "XRI RightHand/Thumbstick"

    [Header("Scale Settings")]
    public float scaleSpeed = 1f;            // Multiplier per second
    public float minScaleFactor = 0.2f;      // 50% of original
    public float maxScaleFactor = 2f;        // 200% of original
    public float deadZone = 0.1f;            // Ignore tiny thumbstick motion

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private Vector3 originalScale;

    private TeleportationProvider teleportationProvider;
    private SnapTurnProvider snapTurnProvider;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        teleportationProvider = Object.FindFirstObjectByType<TeleportationProvider>();
        snapTurnProvider = Object.FindFirstObjectByType<SnapTurnProvider>();
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!enabled) return; // Skip if script is disabled

        isGrabbed = true;
        originalScale = transform.localScale;

        rightThumbstickAction.action.Enable();

        if (teleportationProvider) teleportationProvider.enabled = false;
        if (snapTurnProvider) snapTurnProvider.enabled = false;

        grabInteractable.trackScale = false;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (!enabled) return; // Skip if script is disabled

        isGrabbed = false;
        rightThumbstickAction.action.Disable();

        if (teleportationProvider) teleportationProvider.enabled = true;
        if (snapTurnProvider) snapTurnProvider.enabled = true;
    }


    void Update()
    {
        if (!isGrabbed) return;

        Vector2 axis = rightThumbstickAction.action.ReadValue<Vector2>();

        if (Mathf.Abs(axis.y) > deadZone)
        {
            // Calculate scale factor
            float scaleFactor = 1 + (axis.y * scaleSpeed * Time.deltaTime);

            // Apply scaling relative to original scale
            Vector3 newScale = transform.localScale * scaleFactor;

            // Clamp new scale directly (based on original scale range)
            float min = originalScale.x * minScaleFactor;
            float max = originalScale.x * maxScaleFactor;

            float clamped = Mathf.Clamp(newScale.x, min, max);
            transform.localScale = new Vector3(clamped, clamped, clamped);
        }
    }
}