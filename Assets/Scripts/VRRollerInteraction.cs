using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRRollerInteraction : MonoBehaviour
{
    public int rollerIndex; // Set this in the inspector or during runtime
    public MoveRuller moveRullerScript;

    private Quaternion lastRotation;
    private bool isGrabbed = false;

    void Start()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        lastRotation = transform.rotation;
        isGrabbed = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed) return;

        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        float angle;
        Vector3 axis;

        deltaRotation.ToAngleAxis(out angle, out axis);

        if (Mathf.Abs(Vector3.Dot(axis, transform.right)) > 0.7f && angle > 1f)
        {
            // We only care about rotation along the X axis
            int direction = Vector3.Dot(axis, transform.right) > 0 ? 1 : -1;

            transform.Rotate(direction * 36f, 0f, 0f, Space.Self);
            moveRullerScript.AdjustRollerNumber(rollerIndex, -direction); // Negative because direction is reversed

            lastRotation = transform.rotation;
        }
    }
}
