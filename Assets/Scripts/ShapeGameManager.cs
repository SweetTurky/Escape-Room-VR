using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ShapeGameManager : MonoBehaviour
{
    [Header("Shape Prefabs (3D Models)")]
    public ShapePrefab[] shapePrefabs; // Assign Cube, Sphere, etc. prefabs in inspector

    [Header("Holes")]
    public Hole[] holes; // 3 elements, each representing a hole

    [Header("Game Settings")]
    public int maxRounds = 3;

    private int currentRound = 0;

    [Header("Shape Spawner")]
    public ShapeSpawner shapeSpawner;

    [Serializable]
    public class Hole
    {
        public string targetShapeName;

        [Header("Shape Display")]
        public Transform shapeModelSpawnPoint;     // Empty GameObject above hole
        public GameObject currentModelInstance;    // Instantiated 3D model
        public GameObject checkmark;               // Checkmark GameObject
        public GameObject redCross;                // Red cross GameObject

        [Header("Trigger")]
        public TriggerArea triggerArea;
    }

    [Serializable]
    public class ShapePrefab
    {
        public string shapeName;
        public GameObject prefab;
    }

    void Start()
    {
        AssignRandomTargets();
    }

    public void AssignRandomTargets()
    {
        // Pick 3 random shapes
        var selectedShapes = shapePrefabs.OrderBy(x => UnityEngine.Random.value).Take(3).ToArray();

        for (int i = 0; i < holes.Length; i++)
        {
            Hole hole = holes[i];
            ShapePrefab selectedShape = selectedShapes[i];

            // Set target name
            hole.targetShapeName = selectedShape.shapeName;

            // Clear previous model
            if (hole.currentModelInstance != null)
                Destroy(hole.currentModelInstance);

            // Spawn new shape model above the hole
            hole.currentModelInstance = Instantiate(
                selectedShape.prefab,
                hole.shapeModelSpawnPoint.position,
                Quaternion.identity,
                hole.shapeModelSpawnPoint
            );

            // Apply uniform scale reduction
            hole.currentModelInstance.transform.localScale *= 0.25f;

            // Reset rotation in case the prefab has one
            hole.currentModelInstance.transform.localRotation = Quaternion.identity;

            // Add bop animation
            if (!hole.currentModelInstance.GetComponent<HoverBop>())
                hole.currentModelInstance.AddComponent<HoverBop>();

            // Reset feedback visuals
            hole.checkmark.SetActive(false);
            hole.redCross.SetActive(false);

            // Let trigger know expected shape
            hole.triggerArea.SetTargetShape(selectedShape.shapeName);
        }

        // === NEW: Refresh shape objects for this round ===
        if (shapeSpawner != null)
        {
            shapeSpawner.ClearAllShapes(); // Destroy any leftover shapes
            shapeSpawner.SpawnShapes();    // Spawn fresh set of physical shapes
        }
    }


    public void HoleCorrect(Hole hole)
    {
        hole.checkmark.SetActive(true);

        if (holes.All(h => h.checkmark.activeSelf))
        {
            currentRound++;

            if (currentRound < maxRounds)
            {
                Invoke(nameof(AssignRandomTargets), 1.5f);
            }
            else
            {
                Debug.Log("Game Over: All rounds complete!");
                // Call any end-game logic here
            }
        }
    }

    public void HoleIncorrect(Hole hole)
    {
        StartCoroutine(FlashRedCross(hole.redCross));
    }

    private IEnumerator FlashRedCross(GameObject redCross)
    {
        redCross.SetActive(true);
        yield return new WaitForSeconds(1f);
        redCross.SetActive(false);
    }
}
