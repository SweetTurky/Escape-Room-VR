using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public int currentRoom;
    public float playerX, playerY, playerZ;
    public float playerRotX, playerRotY, playerRotZ;
    public bool door1Open, door2Open, door3Open;
    public bool hasKey;
    public float elapsedTime;
}

public class SaveSystem : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/savegame.json";

    /// <summary>
    /// Saves the current game state, player position, and relevant objects.
    /// </summary>
    public static void SaveGame(GameStateManager gameStateManager, Transform player, bool door1Open, bool door2Open, bool door3Open, bool hasKey, float elapsedTime)
    {
        SaveData data = new SaveData
        {
            currentRoom = (int)gameStateManager.currentState,
            playerX = player.position.x,
            playerY = player.position.y,
            playerZ = player.position.z,
            playerRotX = player.rotation.eulerAngles.x,
            playerRotY = player.rotation.eulerAngles.y,
            playerRotZ = player.rotation.eulerAngles.z,
            door1Open = door1Open,
            door2Open = door2Open,
            door3Open = door3Open,
            hasKey = hasKey,
            elapsedTime = elapsedTime
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game Saved Successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error Saving Game: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the saved game state if a save file exists.
    /// </summary>
    public static SaveData LoadGame()
    {
        if (!SaveExists())
        {
            Debug.LogWarning("No save file found. Starting new game.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error Loading Game: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }

    /// <summary>
    /// Deletes the save file, resetting the game progress.
    /// </summary>
    public static void DeleteSave()
    {
        if (SaveExists())
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted. Game progress reset.");
        }
    }
}
