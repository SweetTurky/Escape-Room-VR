using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ShapeSpawner : MonoBehaviour
{
    [Header("Initial Objects In Scene")]
    public GameObject[] initialShapeObjects;

    [Header("Spawn Configuration")]
    public Transform[] spawnPoints; // Positions where shapes can appear
    public GameObject[] shapePrefabs; // Prefabs tagged with shape names (e.g., "Cube", "Sphere")

    [Header("Manager Reference")]
    public ShapeGameManager manager;

    // Internal tracking
    private List<Transform> usedSpawnPoints = new List<Transform>();
    private List<GameObject> spawnedShapes = new List<GameObject>();
    private List<GameObject> activeShapeObjects = new List<GameObject>(); // Tracks both initial + spawned

    void Start()
    {
        // Track initial scene objects
        activeShapeObjects.AddRange(initialShapeObjects);
    }

    // === Public API ===

    /// <summary>
    /// Spawn one instance of each shape prefab at free spawn points.
    /// </summary>
    public void SpawnShapes()
    {
        ClearSpawnedShapes();

        foreach (GameObject prefab in shapePrefabs)
        {
            Transform spawnPoint = GetFreeSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("No free spawn points left.");
                break;
            }

            GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            spawnedShapes.Add(instance);
            activeShapeObjects.Add(instance);

            EnableGrabComponents(instance);
        }
    }

    /// <summary>
    /// Spawn a specific shape by tag name at a free spawn point.
    /// </summary>
    public void SpawnShape(string shapeName)
    {
        GameObject prefab = shapePrefabs.FirstOrDefault(p => p.CompareTag(shapeName));
        Transform spawnPoint = GetFreeSpawnPoint();

        if (prefab != null && spawnPoint != null)
        {
            GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            spawnedShapes.Add(instance);
            activeShapeObjects.Add(instance);

            EnableGrabComponents(instance);
        }
        else
        {
            Debug.LogWarning($"Cannot spawn shape: {shapeName} — prefab or spawn point missing.");
        }
    }

    /// <summary>
    /// Called by TriggerArea when a shape is used.
    /// </summary>
    public void RemoveShape(GameObject obj)
    {
        if (activeShapeObjects.Contains(obj))
        {
            activeShapeObjects.Remove(obj);
            spawnedShapes.Remove(obj); // In case it's also a spawned one
            Destroy(obj);
        }
    }

    /// <summary>
    /// Clears only spawned (not initial) shapes.
    /// </summary>
    public void ClearSpawnedShapes()
    {
        foreach (GameObject obj in spawnedShapes)
        {
            if (obj != null)
                Destroy(obj);
        }

        activeShapeObjects.RemoveAll(obj => spawnedShapes.Contains(obj));
        spawnedShapes.Clear();
        usedSpawnPoints.Clear();
    }

    /// <summary>
    /// Clears all shapes — both spawned and initial. Use for full reset.
    /// </summary>
    public void ClearAllShapes()
    {
        foreach (GameObject obj in activeShapeObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        activeShapeObjects.Clear();
        spawnedShapes.Clear();
        usedSpawnPoints.Clear();
    }

    /// <summary>
    /// Check if the player has used up all current shape objects.
    /// </summary>
    public bool AreAllShapesUsed()
    {
        return activeShapeObjects.Count == 0;
    }

    // === Internal Helpers ===

    private Transform GetFreeSpawnPoint()
    {
        foreach (Transform point in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(point))
            {
                usedSpawnPoints.Add(point);
                return point;
            }
        }
        return null;
    }

    private void EnableGrabComponents(GameObject instance)
    {
        var grab = instance.GetComponent<XRGrabInteractable>();
        if (grab) grab.enabled = true;

        var transformer = instance.GetComponent<XRGeneralGrabTransformer>();
        if (transformer) transformer.enabled = true;
    }
}
