using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ShapeSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    public Transform[] spawnPoints; // Positions where shapes can appear
    public GameObject[] shapePrefabs; // Prefabs tagged with shape names (e.g., "Cube", "Sphere")

    [Header("Manager Reference")]
    public ShapeGameManager manager;

    // Internal tracking
    private List<Transform> usedSpawnPoints = new List<Transform>();
    private List<GameObject> spawnedShapes = new List<GameObject>();

    // Map from spawned shape to its spawn point for reliable freeing
    private Dictionary<GameObject, Transform> shapeToSpawnPoint = new Dictionary<GameObject, Transform>();

    [Tooltip("Choose to use embodied scaling or (right) thumbstick scaling.")]
    public bool thumbstickScale;

    // === Public API ===

    /// <summary>
    /// Spawns one instance of each shape prefab at a free spawn point.
    /// </summary>
    public void SpawnShapes()
    {
        ClearSpawnedShapes();

        foreach (var shape in manager.shapePrefabs)
        {
            Transform spawnPoint = GetFreeSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("No free spawn points left.");
                break;
            }

            GameObject instance = Instantiate(shape.prefab, spawnPoint.position, Quaternion.identity);
            spawnedShapes.Add(instance);

            // Track spawn point for this shape
            shapeToSpawnPoint[instance] = spawnPoint;

            EnableGrabComponents(instance);
        }
    }

    /// <summary>
    /// Spawns a specific shape (by tag name) at a free spawn point.
    /// </summary>
    public void SpawnShape(string shapeName)
    {
        Debug.Log($"[ShapeSpawner] Attempting to spawn shape: {shapeName}");

        GameObject prefab = manager.shapePrefabs
            .FirstOrDefault(p => p.shapeName == shapeName)?.prefab;

        if (prefab == null)
        {
            Debug.LogWarning($"[ShapeSpawner] No prefab found for shapeName: {shapeName}");
            return;
        }

        Transform spawnPoint = GetFreeSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("[ShapeSpawner] No available spawn point found.");
            return;
        }

        GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        spawnedShapes.Add(instance);

        // Track spawn point for this shape
        shapeToSpawnPoint[instance] = spawnPoint;

        Debug.Log($"[ShapeSpawner] Spawned shape '{shapeName}' at {spawnPoint.position}");

        EnableGrabComponents(instance);
    }

    /// <summary>
    /// Called by TriggerArea when a shape is used (correct or incorrect).
    /// Removes shape and frees its spawn point.
    /// </summary>
    public void RemoveShape(GameObject obj)
    {
        if (spawnedShapes.Contains(obj))
        {
            spawnedShapes.Remove(obj);

            if (shapeToSpawnPoint.TryGetValue(obj, out Transform spawnPoint))
            {
                if (usedSpawnPoints.Contains(spawnPoint))
                {
                    usedSpawnPoints.Remove(spawnPoint);
                    Debug.Log($"[ShapeSpawner] Freed spawn point at {spawnPoint.position}");
                }
                shapeToSpawnPoint.Remove(obj);
            }
            else
            {
                // Fallback: use distance check as before
                Transform closestPoint = spawnPoints
                    .FirstOrDefault(p => Vector3.Distance(p.position, obj.transform.position) < 0.1f);

                if (closestPoint != null && usedSpawnPoints.Contains(closestPoint))
                {
                    usedSpawnPoints.Remove(closestPoint);
                    Debug.Log($"[ShapeSpawner] Freed spawn point at {closestPoint.position} (fallback)");
                }
            }

            Destroy(obj);
        }
        else
        {
            Debug.LogWarning("[ShapeSpawner] Tried to remove shape that is not tracked.");
            Destroy(obj);
        }
    }

    /// <summary>
    /// Destroys all currently spawned shape objects.
    /// </summary>
    public void ClearSpawnedShapes()
    {
        foreach (GameObject obj in spawnedShapes)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedShapes.Clear();
        usedSpawnPoints.Clear();
        shapeToSpawnPoint.Clear();

        Debug.Log("[ShapeSpawner] Cleared all spawned shapes and freed spawn points.");
    }

    /// <summary>
    /// Checks if all spawned shape objects have been used.
    /// </summary>
    public bool AreAllShapesUsed()
    {
        return spawnedShapes.Count == 0;
    }

    // === Internal Helpers ===

    private Transform GetFreeSpawnPoint()
    {
        Debug.Log($"[ShapeSpawner] Used spawn points: {usedSpawnPoints.Count}/{spawnPoints.Length}");

        foreach (Transform point in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(point))
            {
                usedSpawnPoints.Add(point);
                Debug.Log($"[ShapeSpawner] Using spawn point at {point.position}");
                return point;
            }
        }

        Debug.LogWarning("[ShapeSpawner] No free spawn points available!");
        return null;
    }

    private void EnableGrabComponents(GameObject instance)
    {
        var grab = instance.GetComponent<XRGrabInteractable>();
        if (grab) grab.enabled = true;

        var transformer = instance.GetComponent<XRGeneralGrabTransformer>();
        if (transformer) transformer.enabled = true;

        if (thumbstickScale)
        {
            var thumbstickscale = instance.GetComponent<ThumbstickScalerInput>();
            if (thumbstickscale) thumbstickscale.enabled = true;
        }
    }
}

