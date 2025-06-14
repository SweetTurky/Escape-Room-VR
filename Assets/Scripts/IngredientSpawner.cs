using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject jarPrefab;
    public GameObject[] ingredientPrefabs;
    public Transform spawnPoint;

    [Header("Particle System")]
    public ParticleEffectController particleController;

    private void Start()
    {
        SpawnJarWithIngredient(0);
    }

    public void SpawnJarWithIngredient(int ingredientIndex)
    {
        if (jarPrefab == null || ingredientPrefabs.Length == 0 || spawnPoint == null) return;

        // Clamp index to safe range
        ingredientIndex = Mathf.Clamp(ingredientIndex, 0, ingredientPrefabs.Length - 1);
        GameObject ingredientPrefab = ingredientPrefabs[ingredientIndex];

        // 1. Spawn jar
        GameObject jarInstance = Instantiate(jarPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Spawn ingredient inside the jar
        Transform ingredientPoint = jarInstance.transform.Find("IngredientPoint");
        if (ingredientPoint != null)
        {
            // A) Instantiate with no parent so it keeps its prefab scale
            GameObject ingredientInstance = Instantiate(
                ingredientPrefab,
                ingredientPoint.position,
                ingredientPoint.rotation
            );

            // B) Parent it—but preserve world position/rotation/scale
            ingredientInstance.transform.SetParent(ingredientPoint, worldPositionStays: true);

            // C) Snap it exactly into place (position/rotation are already correct,
            //    this is mostly defensive)
            ingredientInstance.transform.localPosition = Vector3.zero;
            ingredientInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("IngredientPoint not found on jar prefab.");
        }

        // 3. Start particles
        if (particleController != null)
        {
            particleController.StartParticles();
        }

        // 4. Watch for grab
        JarGrabWatcher watcher = jarInstance.AddComponent<JarGrabWatcher>();
        watcher.particleController = particleController;
    }
}
