using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CauldronController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The spoon Transform to clamp & track.")]
    public Transform spoonTransform;

    [Tooltip("A trigger Collider (set as 'Is Trigger') defining the stirring zone.")]
    public Collider stirZoneTrigger;

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
    float accumulatedCW = 0f;   // degrees
    float accumulatedCCW = 0f;  // degrees

    int _nextCheckpointIndex = 0;
    int _ingredientsAddedCount = 0;
    bool _potionFinished = false;

    void Start()
    {
        if (stirZoneTrigger == null || spoonTransform == null)
            Debug.LogError("Assign stirZoneTrigger & spoonTransform on " + name);

        if (stirZoneTrigger.gameObject != gameObject)
            Debug.LogWarning("stirZoneTrigger likely should be on the same GameObject.");
    }

    void Update()
    {
        if (!isInStirZone) return;

        ClampSpoonPosition();
        TrackStirring();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == stirZoneTrigger)
        {
            isInStirZone = true;
            previousAngle = GetCurrentHorizontalAngle();
        }
        else if (other.TryGetComponent<Ingredient>(out var ing))
        {
            // 1) Fire per-ingredient event
            OnIngredientAdded.Invoke(ing.ingredientType);

            // 2) Count and check for brew completion
            _ingredientsAddedCount++;
            if (!_potionFinished && _ingredientsAddedCount >= ingredientsRequired)
            {
                _potionFinished = true;
                OnPotionFinished?.Invoke();
            }

            // 3) Remove the ingredient from the world
            Destroy(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == stirZoneTrigger)
            isInStirZone = false;
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
        if (_nextCheckpointIndex >= stirCheckpoints.Length)
            return; // no more checkpoints left

        // Compute how far we've stirred since last frame
        float angle = GetCurrentHorizontalAngle();
        float delta = Mathf.DeltaAngle(previousAngle, angle);
        previousAngle = angle;

        if (Mathf.Abs(delta) < rotationThresholdPerFrame)
            return;

        if (delta < 0)
            accumulatedCW += -delta;
        else
            accumulatedCCW += delta;

        // Check only the *next* checkpoint
        var cp = stirCheckpoints[_nextCheckpointIndex];
        float neededDeg = cp.requiredRotations * 360f;
        bool passed = (cp.direction == StirCheckpoint.Direction.Clockwise && accumulatedCW >= neededDeg)
                   || (cp.direction == StirCheckpoint.Direction.CounterClockwise && accumulatedCCW >= neededDeg);

        if (passed)
        {
            cp.onCheckpointReached.Invoke();
            _nextCheckpointIndex++;
        }
    }

    float GetCurrentHorizontalAngle()
    {
        Vector3 local = transform.InverseTransformPoint(spoonTransform.position);
        return Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        Vector3 bottom = center + Vector3.up * minY;
        Vector3 top = center + Vector3.up * maxY;

        DrawWireCircle(bottom, clampRadius);
        DrawWireCircle(top, clampRadius);

        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (var d in dirs)
        {
            Gizmos.DrawLine(bottom + d * clampRadius, top + d * clampRadius);
        }
    }

    void DrawWireCircle(Vector3 center, float radius)
    {
        const int segments = 36;
        float deltaTheta = 2f * Mathf.PI / segments;
        Vector3 prev = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float theta = i * deltaTheta;
            Vector3 next = center + new Vector3(Mathf.Cos(theta) * radius, 0, Mathf.Sin(theta) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
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
