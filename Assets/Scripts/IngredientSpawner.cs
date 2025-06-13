using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject jarPrefab;
    public GameObject[] ingredientPrefabs;
    public Transform spawnPoint;

    [Header("Particle System")]
    public ParticleEffectController particleController;

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
            Instantiate(ingredientPrefab, ingredientPoint.position, ingredientPoint.rotation, ingredientPoint);
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
