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

    [Header("Typed Enter Events (fire in order)")]
    public UnityEvent<StationType>[] OnPlayerEnter;

    [Header("Typed Exit Events (fire in order)")]
    public UnityEvent<StationType>[] OnPlayerExit;

    [Header("Simple Enter Events (no params, fire in order)")]
    public UnityEvent[] onPlayerEnterSimple;

    [Header("Simple Exit Events (no params, fire in order)")]
    public UnityEvent[] onPlayerExitSimple;

    // internal
    private bool _playerInside = false;
    private int _enterIndex = 0;
    private int _exitIndex = 0;

    private void Reset()
    {
        // ensure the Collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || _playerInside) return;
        _playerInside = true;

        // fire the next typed-enter event, if any
        if (_enterIndex < OnPlayerEnter.Length && OnPlayerEnter[_enterIndex] != null)
            OnPlayerEnter[_enterIndex].Invoke(stationType);

        // fire the next simple-enter event, if any
        if (_enterIndex < onPlayerEnterSimple.Length && onPlayerEnterSimple[_enterIndex] != null)
            onPlayerEnterSimple[_enterIndex].Invoke();

        // advance so this slot never fires again
        _enterIndex++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInside = false;

        // fire the next typed-exit event, if any
        if (_exitIndex < OnPlayerExit.Length && OnPlayerExit[_exitIndex] != null)
            OnPlayerExit[_exitIndex].Invoke(stationType);

        // fire the next simple-exit event, if any
        if (_exitIndex < onPlayerExitSimple.Length && onPlayerExitSimple[_exitIndex] != null)
            onPlayerExitSimple[_exitIndex].Invoke();

        // advance so this slot never fires again
        _exitIndex++;
    }
}
