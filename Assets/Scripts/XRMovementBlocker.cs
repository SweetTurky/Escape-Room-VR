using UnityEngine;

public class XRMovementBlocker : MonoBehaviour, IXRMovementBlockable
{
    [Tooltip("E.g. ActionBasedContinuousMoveProvider")]
    public MonoBehaviour movementProviderToDisable;

    [Tooltip("E.g. ActionBasedContinuousTurnProvider")]
    public MonoBehaviour turnProviderToDisable;

    public void EnableMovement(bool enabled)
    {
        if (movementProviderToDisable != null)
            movementProviderToDisable.enabled = enabled;
        if (turnProviderToDisable != null)
            turnProviderToDisable.enabled = enabled;
    }
}
