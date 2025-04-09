using UnityEngine;
using System;

public class RoomController : MonoBehaviour
{
    public GameObject door; // Assign the door that opens this room
    public AudioSource completionSound; // Assign a completion sound effect

    public event Action OnRoomCompleted; // Event triggered when criteria are met

    private bool isRoomOpen = false; // Tracks if the room is open

    public bool IsDoorOpen => isRoomOpen; // Property to expose door status (used for Save/Load)

    /// <summary>
    /// Opens the room by animating the door.
    /// </summary>
    public void OpenRoom()
    {
        if (!isRoomOpen)
        {
            isRoomOpen = true;
            if (door != null)
            {
                TransitionManager.Instance?.OpenDoor(door); // Animate door
                Debug.Log($"{gameObject.name} is now open.");
            }
            else
            {
                Debug.LogWarning($"Door not assigned for {gameObject.name}.");
            }
        }
    }

    /// <summary>
    /// Called when the room's puzzle criteria are met.
    /// </summary>
    public void OnCriteriaMet()
    {
        if (!isRoomOpen) // Ensures event fires only once
        {
            isRoomOpen = true;

            if (completionSound != null)
            {
                completionSound.Play(); // Play completion sound
            }

            Debug.Log($"{gameObject.name} completed!");
            TransitionManager.Instance?.DisplayFeedback($"{gameObject.name} Completed!");

            OnRoomCompleted?.Invoke(); // Notify GameStateManager
        }
    }

    /// <summary>
    /// Resets the room state (used for Save/Load functionality).
    /// </summary>
    public void ResetRoom(bool shouldOpen)
    {
        isRoomOpen = shouldOpen;

        if (shouldOpen)
        {
            OpenRoom(); // Re-open the door if the room was open before saving
        }
        else
        {
            Debug.Log($"{gameObject.name} reset to closed state.");
        }
    }
}
