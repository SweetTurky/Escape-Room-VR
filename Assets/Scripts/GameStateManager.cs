using UnityEngine;

public enum GameState
{
    Room1,
    Room2,
    Room3,
    Completed
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; } // Singleton reference

    public GameState currentState = GameState.Room1;

    public Transform player; // Assign Player in Inspector
    public RoomController room1Controller;
    public RoomController room2Controller;
    public RoomController room3Controller;

    public bool hasKey = false;
    public float elapsedTime = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SubscribeToEvents();
        LoadGame(); // Load saved state if available
        UpdateState();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (room1Controller != null)
            room1Controller.OnRoomCompleted += OnRoom1Completed;
        if (room2Controller != null)
            room2Controller.OnRoomCompleted += OnRoom2Completed;
        if (room3Controller != null)
            room3Controller.OnRoomCompleted += OnRoom3Completed;
    }

    private void UnsubscribeFromEvents()
    {
        if (room1Controller != null)
            room1Controller.OnRoomCompleted -= OnRoom1Completed;
        if (room2Controller != null)
            room2Controller.OnRoomCompleted -= OnRoom2Completed;
        if (room3Controller != null)
            room3Controller.OnRoomCompleted -= OnRoom3Completed;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime; // Track time
    }

    private void OnRoom1Completed()
    {
        if (currentState == GameState.Room1)
            AdvanceState();
    }

    private void OnRoom2Completed()
    {
        if (currentState == GameState.Room2)
            AdvanceState();
    }

    private void OnRoom3Completed()
    {
        if (currentState == GameState.Room3)
            AdvanceState();
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        SaveGame();
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case GameState.Room1:
                room1Controller?.OpenRoom();
                break;
            case GameState.Room2:
                room1Controller?.OpenRoom();
                room2Controller?.OpenRoom();
                break;
            case GameState.Room3:
                room1Controller?.OpenRoom();
                room2Controller?.OpenRoom();
                room3Controller?.OpenRoom();
                break;
            case GameState.Completed:
                Debug.Log("Escape Room Completed!");
                TransitionManager.Instance?.DisplayFeedback("Congratulations! Escape Room Completed!");
                break;
        }
    }

    public void AdvanceState()
    {
        if (currentState == GameState.Room1)
            SetState(GameState.Room2);
        else if (currentState == GameState.Room2)
            SetState(GameState.Room3);
        else if (currentState == GameState.Room3)
            SetState(GameState.Completed);
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(this, player,
            room1Controller.IsDoorOpen,
            room2Controller.IsDoorOpen,
            room3Controller.IsDoorOpen,
            hasKey, elapsedTime);
    }

    public void LoadGame()
    {
        UnsubscribeFromEvents(); // Prevent duplicate event subscriptions

        SaveData data = SaveSystem.LoadGame();
        if (data != null)
        {
            currentState = (GameState)data.currentRoom;
            player.position = new Vector3(data.playerX, data.playerY, data.playerZ);
            player.rotation = Quaternion.Euler(data.playerRotX, data.playerRotY, data.playerRotZ);

            room1Controller.ResetRoom(data.door1Open);
            room2Controller.ResetRoom(data.door2Open);
            room3Controller.ResetRoom(data.door3Open);

            hasKey = data.hasKey;
            elapsedTime = data.elapsedTime;

            SubscribeToEvents(); // Re-subscribe after loading
            UpdateState(); // Apply loaded data
        }
        else
        {
            Debug.LogWarning("No saved game found!");
        }
    }

    public void ResetGame()
    {
        SaveSystem.DeleteSave();
        SetState(GameState.Room1);
    }
}
