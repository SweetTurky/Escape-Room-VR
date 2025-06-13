using UnityEngine;

public class XRMovementBlocker : MonoBehaviour, IXRMovementBlockable
{
    public MonoBehaviour movementProviderToDisable; // E.g., ActionBasedContinuousMoveProvider

    public void EnableMovement(bool enabled)
    {
        if (movementProviderToDisable != null)
        {
            movementProviderToDisable.enabled = enabled;
        }
    }
}
