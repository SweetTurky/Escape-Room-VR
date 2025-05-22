using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class ShapeGameManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip introVoiceLine;
    public AudioSource audioSource;

    [Header("Fade Controller")]
    public VRFade vrFade; // Assign this in the Inspector

    [Header("Subtitles Manager")]
    public VRSubtitles subtitlesManager;

    [Header("Fade Settings")]
    [Tooltip("Duration of the fade-in effect (seconds)")]
    public float fadeInDuration = 4f;
    [Tooltip("Duration of the fade-out effect (seconds)")]
    public float fadeOutDuration = 3f;

    [Header("Shape Prefabs (3D Models)")]
    public ShapePrefab[] shapePrefabs; // Assign Cube, Sphere, etc. prefabs in inspector

    [Header("Holes")]
    public Hole[] holes; // 3 elements, each representing a hole

    [Header("Game Settings")]
    public int maxRounds = 3;

    private int currentRound = 0;

    [Header("Shape Spawner")]
    public ShapeSpawner shapeSpawner;

    
    private List<string> remainingTargetShapes = new List<string>();

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
        //StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(3.5f);

        // Start playing the voice line right away if available
        if (audioSource != null && introVoiceLine != null)
        {
            audioSource.clip = introVoiceLine;
            audioSource.Play();

            if (subtitlesManager != null)
            {
                subtitlesManager.StartSubtitles(audioSource);
            }
        }

        // Fade from black to clear
        if (vrFade != null)
        {
            vrFade.SetAlpha(1f);            // Ensure fully black to start
            vrFade.FadeIn(fadeInDuration);  // Begin fading in
            yield return new WaitForSeconds(fadeInDuration); // Wait for fade-in to complete
        }

        yield return new WaitForSeconds(3f); // Wait for voiceline description to match what is happening
        currentRound = 0;
        AssignRandomTargets();
    }


    private IEnumerator EndGameRoutine()
    {
        if (vrFade != null)
        {
            vrFade.FadeOut(2f);     // Fade to black over 2 seconds
            yield return new WaitForSeconds(2f);
        }
        Debug.Log("Game Over");
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "SUI")
        {
            // Load SUI2
            SceneManager.LoadScene("SUI2");
        }
        else if (currentSceneName == "SUI2")
        {
            // Quit application
            Application.Quit();
            // In the editor, Application.Quit does not quit, so we log a message
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        // Add any additional post-fade end game logic here
    }


    public void AssignRandomTargets()
    {
        // Pick 3 random shapes
        var selectedShapes = shapePrefabs.OrderBy(x => UnityEngine.Random.value).Take(3).ToArray();

        remainingTargetShapes = holes.Select(h => h.targetShapeName).ToList();

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
            shapeSpawner.ClearSpawnedShapes(); // Destroy any leftover shapes
            shapeSpawner.SpawnShapes();    // Spawn fresh set of physical shapes
        }
    }


    public void HoleCorrect(Hole hole)
    {
        hole.checkmark.SetActive(true);
        remainingTargetShapes.Remove(hole.targetShapeName);

        if (holes.All(h => h.checkmark.activeSelf))
        {
            currentRound++;

            if (currentRound < maxRounds)
            {
                Invoke(nameof(AssignRandomTargets), 1.5f);
            }
            else
            {
                // Call any end-game logic here
                Debug.Log("Game Over: All rounds complete!");
                StartCoroutine(EndGameRoutine());
            }
        }
    }

    public void HoleIncorrect(Hole hole, string incorrectShape)
    {
        StartCoroutine(FlashRedCross(hole.redCross));
        shapeSpawner.SpawnShape(incorrectShape); // Immediately respawn the wrongly used shape
    }

    private IEnumerator FlashRedCross(GameObject redCross)
    {
        redCross.SetActive(true);
        yield return new WaitForSeconds(1f);
        redCross.SetActive(false);
    }

    public bool HasUnmatchedTargets()
    {
        return remainingTargetShapes.Count > 0;
    }

}
