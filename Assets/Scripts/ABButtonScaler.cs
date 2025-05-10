using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ABButtonScaler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference aButtonAction; // XRI RightHand/Primary Button (A)
    public InputActionReference bButtonAction; // XRI RightHand/Secondary Button (B)

    [Header("Scale Settings")]
    public float scaleSpeed = 1f;
    public float minScaleFactor = 0.2f;
    public float maxScaleFactor = 2f;

    private XRGrabInteractable grabInteractable;
    private int grabCount = 0;
    private Vector3 originalScale;
    private bool scaleUpHeld = false;
    private bool scaleDownHeld = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        // Register button press and release events
        aButtonAction.action.started += OnAScaleStarted;
        aButtonAction.action.canceled += OnAScaleCanceled;
        bButtonAction.action.started += OnBScaleStarted;
        bButtonAction.action.canceled += OnBScaleCanceled;

        aButtonAction.action.Enable();
        bButtonAction.action.Enable();
    }

    private void OnDestroy()
    {
        // Remove listeners and disable actions
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);

        aButtonAction.action.started -= OnAScaleStarted;
        aButtonAction.action.canceled -= OnAScaleCanceled;
        bButtonAction.action.started -= OnBScaleStarted;
        bButtonAction.action.canceled -= OnBScaleCanceled;

        aButtonAction.action.Disable();
        bButtonAction.action.Disable();
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        grabCount++;
        if (grabCount == 1)
            originalScale = transform.localScale;

        grabInteractable.trackScale = false;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        grabCount = Mathf.Max(0, grabCount - 1);
        grabInteractable.trackScale = true;
    }

    private void OnAScaleStarted(InputAction.CallbackContext ctx)
    {
        scaleUpHeld = true;
    }

    private void OnAScaleCanceled(InputAction.CallbackContext ctx)
    {
        scaleUpHeld = false;
    }

    private void OnBScaleStarted(InputAction.CallbackContext ctx)
    {
        scaleDownHeld = true;
    }

    private void OnBScaleCanceled(InputAction.CallbackContext ctx)
    {
        scaleDownHeld = false;
    }

    private void Update()
    {
        if (grabCount == 0) return;

        Vector3 currentScale = transform.localScale;
        float scaleFactor = 1f;

        if (scaleUpHeld)
        {
            scaleFactor += scaleSpeed * Time.deltaTime;
        }
        else if (scaleDownHeld)
        {
            scaleFactor -= scaleSpeed * Time.deltaTime;
        }

        if (scaleFactor != 1f)
        {
            Vector3 newScale = currentScale * scaleFactor;
            float min = originalScale.x * minScaleFactor;
            float max = originalScale.x * maxScaleFactor;
            float clamped = Mathf.Clamp(newScale.x, min, max);
            transform.localScale = new Vector3(clamped, clamped, clamped);
        }
    }
}
