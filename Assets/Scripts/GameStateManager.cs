using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    /// <summary>
    /// Fires whenever the current room index changes.
    /// Listeners get the new index (0-based). 
    /// If index == rooms.Count “Completed.”
    /// </summary>
    public event Action<int> OnStateChanged;

    [Tooltip("Drag in all your RoomController components, in the order you want them solved.")]
    public List<RoomController> rooms;

    [Header("Player & Save Data")]
    public Transform player;
    public bool hasKey = false;
    public float elapsedTime = 0f;

    // 0 = Room 0 unlocked, 1 = Room 1 unlocked, …, rooms.Count = Completed
    private int currentRoomIndex = 0;

    // We keep the callbacks so we can unsubscribe later:
    private readonly List<Action> _roomCompletedCbs = new List<Action>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SubscribeToRoomEvents();
        LoadGame();         // will set currentRoomIndex if a save exists
        UpdateState();      // open the right doors
        OnStateChanged?.Invoke(currentRoomIndex);
    }

    private void OnDestroy()
    {
        UnsubscribeFromRoomEvents();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    // Hook up each room’s completion event only once:
    private void SubscribeToRoomEvents()
    {
        _roomCompletedCbs.Clear();
        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            int idx = i; // capture
            Action cb = () => OnRoomCompleted(idx);
            _roomCompletedCbs.Add(cb);
            room.OnRoomCompleted += cb;
        }
    }

    private void UnsubscribeFromRoomEvents()
    {
        for (int i = 0; i < rooms.Count && i < _roomCompletedCbs.Count; i++)
            rooms[i].OnRoomCompleted -= _roomCompletedCbs[i];
    }

    // Only advance if the room we just completed == the one we’re on
    private void OnRoomCompleted(int roomIndex)
    {
        if (roomIndex == currentRoomIndex)
            AdvanceRoom();
    }

    /// <summary>
    /// Jump directly to a given room index (0-based). 
    /// rooms.Count == Completed.
    /// </summary>
    public void SetRoomIndex(int newIndex)
    {
        if (newIndex == currentRoomIndex) return;
        currentRoomIndex = newIndex;
        SaveGame();
        UpdateState();
        OnStateChanged?.Invoke(currentRoomIndex);
    }

    public void AdvanceRoom()
    {
        SetRoomIndex(currentRoomIndex + 1);
    }

    /// <summary>
    /// Opens all rooms up to currentRoomIndex. 
    /// If we’ve gone past the end, fire your “Completed” logic.
    /// </summary>
    private void UpdateState()
    {
        if (currentRoomIndex < rooms.Count)
        {
            for (int i = 0; i <= currentRoomIndex; i++)
                rooms[i].OpenRoom();
        }
        else
        {
            Debug.Log("Escape Room Completed!");
            TransitionManager.Instance?.DisplayFeedback("Congratulations! You’ve escaped!");
        }
    }

    /// <summary>
    /// Collects door-open flags from each room and saves
    /// </summary>
    public void SaveGame()
    {
        // Gather door states into a bool[] instead of List<bool>
        bool[] doorStates = rooms
            .Select(r => r.IsDoorOpen)
            .ToArray();

        SaveSystem.SaveGame(
            this,
            player,
            doorStates,
            hasKey,
            elapsedTime,
            currentRoomIndex
        );
    }

    /// <summary>
    /// Loads and restores player pos, each door, the key, time & room index.
    /// </summary>
    public void LoadGame()
    {
        UnsubscribeFromRoomEvents();

        var data = SaveSystem.LoadGame();
        if (data != null)
        {
            currentRoomIndex = data.currentRoomIndex;

            // restore player
            player.position = new Vector3(data.playerX, data.playerY, data.playerZ);
            player.rotation = Quaternion.Euler(data.playerRotX, data.playerRotY, data.playerRotZ);

            // restore each room’s door
            for (int i = 0; i < rooms.Count; i++)
            {
                bool open = (i < data.doorsOpen.Length) && data.doorsOpen[i];
                rooms[i].ResetRoom(open);
            }

            hasKey = data.hasKey;
            elapsedTime = data.elapsedTime;
        }
        else
        {
            Debug.LogWarning("No saved game found, starting fresh.");
        }

        SubscribeToRoomEvents();
        UpdateState();
        OnStateChanged?.Invoke(currentRoomIndex);
    }

    /// <summary>
    /// Clears the save file and restarts at room 0.
    /// </summary>
    public void ResetGame()
    {
        SaveSystem.DeleteSave();
        SetRoomIndex(0);
    }
}
