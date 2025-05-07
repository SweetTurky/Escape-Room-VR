using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Transform point;
        public string shapeName;
    }

    public ShapeGameManager manager;
    public SpawnPoint[] spawnPoints;
    public GameObject[] shapePrefabs; // Match shapeName tags

    private List<GameObject> spawnedShapes = new List<GameObject>();

    public void SpawnShapes()
    {
        ClearAllShapes();

        foreach (var spawn in spawnPoints)
        {
            GameObject prefab = shapePrefabs.FirstOrDefault(p => p.CompareTag(spawn.shapeName));
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, spawn.point.position, Quaternion.identity);
                spawnedShapes.Add(instance);
            }
        }
    }

    public void RemoveShape(GameObject obj)
    {
        if (spawnedShapes.Contains(obj))
        {
            spawnedShapes.Remove(obj);
            Destroy(obj);
        }
    }

    public void ClearAllShapes()
    {
        foreach (var obj in spawnedShapes)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedShapes.Clear();
    }
}
