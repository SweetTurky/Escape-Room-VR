using UnityEngine;
using System;
using System.IO;

[Serializable]
public class SaveData
{
    // 0-based index of the current room; if == doorsOpen.Length => Completed
    public int currentRoomIndex;

    // Player transform
    public float playerX, playerY, playerZ;
    public float playerRotX, playerRotY, playerRotZ;

    // Dynamic array: one element per room’s door state
    public bool[] doorsOpen;

    // Other game state
    public bool hasKey;
    public float elapsedTime;
}

public class SaveSystem : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/savegame.json";

    /// <summary>
    /// Saves the full game state, including dynamic door flags.
    /// </summary>
    /// <param name="gameStateManager">Your manager (to get room index)</param>
    /// <param name="player">Player Transform</param>
    /// <param name="doorsOpen">Array of door-open states (length == number of rooms)</param>
    /// <param name="hasKey">Whether the player has the key</param>
    /// <param name="elapsedTime">Total elapsed time</param>
    public static void SaveGame(
        GameStateManager gameStateManager,
        Transform player,
        bool[] doorsOpen,
        bool hasKey,
        float elapsedTime,
        int currentRoomIndex
    )
    {
        var data = new SaveData
        {
            currentRoomIndex = currentRoomIndex,

            playerX = player.position.x,
            playerY = player.position.y,
            playerZ = player.position.z,
            playerRotX = player.rotation.eulerAngles.x,
            playerRotY = player.rotation.eulerAngles.y,
            playerRotZ = player.rotation.eulerAngles.z,

            doorsOpen = doorsOpen,
            hasKey = hasKey,
            elapsedTime = elapsedTime
        };

        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game Saved Successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error Saving Game: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the saved game state if it exists, or returns null.
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
        catch (Exception e)
        {
            Debug.LogError($"Error Loading Game: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a save file exists on disk.
    /// </summary>
    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }

    /// <summary>
    /// Deletes the existing save file to reset progress.
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
