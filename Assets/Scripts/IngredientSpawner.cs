using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject jarPrefab;
    public GameObject[] ingredientPrefabs;
    public Transform spawnPoint;

    [Header("Particle System")]
    public ParticleEffectController particleController;

    public void SpawnJarWithIngredient(GameObject ingredientPrefab)
    {
        if (jarPrefab == null || ingredientPrefab == null || spawnPoint == null) return;

        GameObject jarInstance = Instantiate(jarPrefab, spawnPoint.position, spawnPoint.rotation);

        Transform ingredientPoint = jarInstance.transform.Find("IngredientPoint");
        if (ingredientPoint != null)
        {
            Instantiate(ingredientPrefab, ingredientPoint.position, ingredientPoint.rotation, ingredientPoint);
        }
        else
        {
            Debug.LogWarning("IngredientPoint not found.");
        }

        if (particleController != null)
        {
            particleController.StartParticles();
        }

        JarGrabWatcher watcher = jarInstance.AddComponent<JarGrabWatcher>();
        watcher.particleController = particleController;
    }

}
