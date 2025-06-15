using UnityEngine;

[ExecuteAlways]
public class StirringIndicator : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Arrow segment prefab for default stirring.")]
    public GameObject arrowPrefab;
    [Tooltip("Arrow segment prefab for counter-clockwise stirring.")]
    public GameObject arrowCounterClockwisePrefab;
    [Tooltip("If true, uses the counter-clockwise prefab and +90° X-rotation; otherwise uses the default prefab and –90° X-rotation.")]
    public bool useCounterClockwisePrefab = false;

    [Header("Rotation")]
    [Range(0f, 360f)]
    [Tooltip("Degrees per second to spin.")]
    public float rotationSpeed = 45f;
    [Tooltip("True = spin clockwise (Deosil), false = spin counter-clockwise (Widdershins).")]
    public bool counterClockwise = false;

    private Transform _pivot;

    void Start()
    {
        if (!Application.isPlaying)
            return;

        // choose a valid prefab
        if (arrowPrefab == null && arrowCounterClockwisePrefab == null)
        {
            Debug.LogWarning("No arrow prefab assigned.");
            return;
        }

        // create a single pivot under this object
        _pivot = new GameObject("StirPivot").transform;
        _pivot.SetParent(transform, worldPositionStays: false);
        _pivot.localPosition = Vector3.zero;

        // spawn three segments
        for (int i = 0; i < 3; i++)
        {
            // pick which prefab to use
            GameObject prefabToUse = useCounterClockwisePrefab && arrowCounterClockwisePrefab != null
                                     ? arrowCounterClockwisePrefab
                                     : arrowPrefab;

            var arrow = Instantiate(prefabToUse, _pivot);
            arrow.transform.localPosition = Vector3.zero;

            // choose X rotation based on which prefab
            float xRot = useCounterClockwisePrefab ? 90f : -90f;
            // spread around Y by 120° increments
            arrow.transform.localRotation = Quaternion.Euler(xRot, i * 120f, 0f);
        }
    }

    void Update()
    {
        if (!Application.isPlaying || _pivot == null)
            return;

        float dir = counterClockwise ? -1f : 1f;
        // spin around world Y-axis
        _pivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * dir, Space.World);
    }
}
