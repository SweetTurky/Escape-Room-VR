using System.Collections.Generic;
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

    [Header("Typed Enter Events (fire first-then-remove)")]
    public List<UnityEvent<StationType>> OnPlayerEnter;

    [Header("Typed Exit Events (fire first-then-remove)")]
    public List<UnityEvent<StationType>> OnPlayerExit;

    [Header("Simple Enter Events (no params, fire first-then-remove)")]
    public List<UnityEvent> onPlayerEnterSimple;

    [Header("Simple Exit Events (no params, fire first-then-remove)")]
    public List<UnityEvent> onPlayerExitSimple;

    // internal
    private bool _playerInside = false;

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

        // 1) typed enter
        if (OnPlayerEnter.Count > 0 && OnPlayerEnter[0] != null)
        {
            OnPlayerEnter[0].Invoke(stationType);
            OnPlayerEnter.RemoveAt(0);
        }

        // 2) simple enter
        if (onPlayerEnterSimple.Count > 0 && onPlayerEnterSimple[0] != null)
        {
            onPlayerEnterSimple[0].Invoke();
            onPlayerEnterSimple.RemoveAt(0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInside = false;

        // 1) typed exit
        if (OnPlayerExit.Count > 0 && OnPlayerExit[0] != null)
        {
            OnPlayerExit[0].Invoke(stationType);
            OnPlayerExit.RemoveAt(0);
        }

        // 2) simple exit
        if (onPlayerExitSimple.Count > 0 && onPlayerExitSimple[0] != null)
        {
            onPlayerExitSimple[0].Invoke();
            onPlayerExitSimple.RemoveAt(0);
        }
    }
}
