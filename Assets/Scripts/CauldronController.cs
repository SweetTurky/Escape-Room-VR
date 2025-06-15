using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CauldronController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The spoon Transform to clamp & track.")]
    public Transform spoonTransform;

    [Tooltip("Trigger Collider (Is Trigger) defining the stirring zone.")]
    public Collider stirZoneTrigger;

    [Tooltip("Trigger Collider (Is Trigger) defining the ingredient drop zone.")]
    public Collider ingredientZoneTrigger;

    [Header("Clamp Settings")]
    [Tooltip("Radius of the cylindrical zone (in local XZ)")]
    public float clampRadius = 0.5f;
    [Tooltip("Y-axis bounds (local space)")]
    public float minY = 0.2f, maxY = 1.0f;

    [Header("Stirring Settings")]
    [Tooltip("Ignore tiny jitters under this degree delta")]
    public float rotationThresholdPerFrame = 0.5f;
    [Tooltip("Define any number of checkpoints here (in the order they must fire)")]
    public StirCheckpoint[] stirCheckpoints;

    [Header("Ingredient Settings")]
    [Tooltip("Fires when an ingredient is added")]
    public IngredientAddedEvent OnIngredientAdded;

    [Header("Potion Settings")]
    [Tooltip("How many ingredients complete the brew")]
    public int ingredientsRequired = 3;
    [Tooltip("Fires once when the potion is fully brewed")]
    public UnityEvent OnPotionFinished;

    // internal state
    bool isInStirZone = false;
    float previousAngle = 0f;
    float accumulatedCW = 0f, accumulatedCCW = 0f;
    int _nextCheckpointIndex = 0;
    int _ingredientsAddedCount = 0;
    bool _potionFinished = false;

    void Start()
    {
        if (stirZoneTrigger == null || ingredientZoneTrigger == null || spoonTransform == null)
            Debug.LogError("Make sure stirZoneTrigger, ingredientZoneTrigger and spoonTransform are assigned on " + name);
    }

    void Update()
    {
        if (!isInStirZone) return;
        ClampSpoonPosition();
        TrackStirring();
    }

    void OnTriggerEnter(Collider other)
    {
        // Detect the spoon entering by checking its layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Spoon"))
        {
            isInStirZone = true;
            previousAngle = GetCurrentHorizontalAngle();
            Debug.Log("Spoon entered stir zone.");
        }

        // 2) Ingredient drop logic
        if (other.TryGetComponent<Ingredient>(out var ing))
        {
            // only trigger if the ingredient is inside the ingredientZoneTrigger bounds
            if (ingredientZoneTrigger.bounds.Contains(other.transform.position))
            {
                if (ing.GetComponent<ProcessedMarker>() != null)
                {
                    Debug.Log($"CauldronController: {ing.name} already processed, ignoring.");
                    return;
                }

                ing.gameObject.AddComponent<ProcessedMarker>();
                Debug.Log($"CauldronController: Processing ingredient {ing.name}");

                OnIngredientAdded.Invoke(ing.ingredientType);
                _ingredientsAddedCount++;

                /*if (!_potionFinished && _ingredientsAddedCount >= ingredientsRequired)
                {
                    _potionFinished = true;
                    OnPotionFinished?.Invoke();
                }*/

                Destroy(ing.gameObject, 0.1f);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Spoon"))
        {
            isInStirZone = false;
            Debug.Log("Spoon exited stir zone.");
        }
    }

    void ClampSpoonPosition()
    {
        Vector3 local = transform.InverseTransformPoint(spoonTransform.position);
        Vector2 radiusVec = new Vector2(local.x, local.z);
        if (radiusVec.magnitude > clampRadius)
            radiusVec = radiusVec.normalized * clampRadius;
        local.x = radiusVec.x;
        local.z = radiusVec.y;
        local.y = Mathf.Clamp(local.y, minY, maxY);
        spoonTransform.position = transform.TransformPoint(local);
    }

    void TrackStirring()
    {
        if (_nextCheckpointIndex >= stirCheckpoints.Length) return;
        if (_nextCheckpointIndex >= _ingredientsAddedCount) return;

        float angle = GetCurrentHorizontalAngle();
        float delta = Mathf.DeltaAngle(previousAngle, angle);
        previousAngle = angle;

        Debug.Log($"Angle delta: {delta}");

        if (Mathf.Abs(delta) < rotationThresholdPerFrame) return;

        if (delta < 0)
            accumulatedCW += -delta;
        else
            accumulatedCCW += delta;

        Debug.Log($"CW: {accumulatedCW}, CCW: {accumulatedCCW}");

        var cp = stirCheckpoints[_nextCheckpointIndex];
        float neededDeg = cp.requiredRotations * 360f;

        bool passed = (cp.direction == StirCheckpoint.Direction.CounterClockwise && accumulatedCW >= neededDeg)
                   || (cp.direction == StirCheckpoint.Direction.Clockwise && accumulatedCCW >= neededDeg);

        if (passed)
        {
            cp.onCheckpointReached.Invoke();
            Debug.Log($"Checkpoint {_nextCheckpointIndex} reached.");
            _nextCheckpointIndex++;

            // Reset accumulators after checkpoint reached (optional but usually good)
            accumulatedCW = accumulatedCCW = 0f;
        }
    }

    float GetCurrentHorizontalAngle()
    {
        Vector3 local = transform.InverseTransformPoint(spoonTransform.position);
        return Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
    }

    public class ProcessedMarker : MonoBehaviour { }
}

#region Supporting Types

[Serializable]
public class StirCheckpoint
{
    public enum Direction { Clockwise, CounterClockwise }
    public Direction direction;
    public float requiredRotations = 1f;
    public UnityEvent onCheckpointReached;
}

[Serializable]
public class IngredientAddedEvent : UnityEvent<IngredientType> { }

#endregion
