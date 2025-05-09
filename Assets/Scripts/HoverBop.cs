using UnityEngine;

public class HoverBop : MonoBehaviour
{
    [Header("Bop Settings")]
    public float amplitude = 0.05f;
    public float frequency = 1f; // constant for all
    private float phaseOffset;

    [Header("Rotation Settings")]
    public float rotationSpeed = 3f; // Adjusted for smoother rotation

    private Vector3 startPos;

    void Start()
    {
        // Disable Rigidbody if present
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;  // Alternatively, rb.enabled = false; but setting isKinematic is safer to avoid physics issues

        // Disable Collider if present
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        startPos = transform.localPosition;

        // Unique bop phase offset (per object)
        phaseOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // Consistent bop speed, with offset
        float yOffset = Mathf.Sin(Time.time * frequency + phaseOffset) * amplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);

        // Consistent rotation around the Y-axis (or any single axis)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
