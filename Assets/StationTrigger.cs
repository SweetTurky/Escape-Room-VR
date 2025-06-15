using UnityEngine;
using UnityEngine.Events;

public enum StationType
{
    IngredientStation,
    CauldronStation,
    // add more types if you need
}

[RequireComponent(typeof(Collider))]
public class StationTrigger : MonoBehaviour
{
    [Tooltip("Which station is this?")]
    public StationType stationType;

    [Tooltip("Tag used to identify the player (e.g. your XR Rig root or a child collider)")]
    public string playerTag = "Player";

    [Header("Events")]
    public UnityEvent<StationType> OnPlayerEnter;
    public UnityEvent<StationType> OnPlayerExit;

    private void Reset()
    {
        // ensure the Collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            OnPlayerEnter?.Invoke(stationType);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            OnPlayerExit?.Invoke(stationType);
    }
}
