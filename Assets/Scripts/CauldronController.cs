using System;
using System.Collections.Generic;
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
    [Tooltip("Define any number of checkpoints here")]
    public StirCheckpoint[] stirCheckpoints;

    [Header("Ingredient Settings")]
    [Tooltip("Fires when an ingredient is added")]
    public IngredientAddedEvent OnIngredientAdded;

    // internal state
    bool isInStirZone = false;
    float previousAngle = 0f;
    float accumulatedCW = 0f;   // degrees
    float accumulatedCCW = 0f;  // degrees
    HashSet<StirCheckpoint> triggered = new HashSet<StirCheckpoint>();

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
            OnIngredientAdded.Invoke(ing.ingredientType);
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
        float angle = GetCurrentHorizontalAngle();
        float delta = Mathf.DeltaAngle(previousAngle, angle);
        previousAngle = angle;

        if (Mathf.Abs(delta) < rotationThresholdPerFrame) return;

        if (delta < 0)
            accumulatedCW += -delta;
        else
            accumulatedCCW += delta;

        foreach (var cp in stirCheckpoints)
        {
            if (triggered.Contains(cp)) continue;
            float degNeeded = cp.requiredRotations * 360f;
            if (cp.direction == StirCheckpoint.Direction.Clockwise && accumulatedCW >= degNeeded)
            {
                cp.onCheckpointReached.Invoke();
                triggered.Add(cp);
            }
            else if (cp.direction == StirCheckpoint.Direction.CounterClockwise && accumulatedCCW >= degNeeded)
            {
                cp.onCheckpointReached.Invoke();
                triggered.Add(cp);
            }
        }
    }

    float GetCurrentHorizontalAngle()
    {
        Vector3 local = transform.InverseTransformPoint(spoonTransform.position);
        return Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
    }

    // Draw clamp gizmos in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;

        // Draw bottom circle at minY
        Vector3 bottom = center + Vector3.up * minY;
        DrawWireCircle(bottom, clampRadius);

        // Draw top circle at maxY
        Vector3 top = center + Vector3.up * maxY;
        DrawWireCircle(top, clampRadius);

        // Draw vertical lines at cardinal directions
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (var d in dirs)
        {
            Vector3 start = bottom + d * clampRadius;
            Vector3 end = top + d * clampRadius;
            Gizmos.DrawLine(start, end);
        }
    }

    // Helper to draw a horizontal wire circle
    void DrawWireCircle(Vector3 center, float radius)
    {
        const int segments = 36;
        float deltaTheta = 2f * Mathf.PI / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float theta = i * deltaTheta;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(theta) * radius, 0, Mathf.Sin(theta) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
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
