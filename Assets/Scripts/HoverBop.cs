using UnityEngine;

public class HoverBop : MonoBehaviour
{
    [Header("Bop Settings")]
    public float amplitude = 0.05f;
    public float minFrequency = 0.8f;
    public float maxFrequency = 1.2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    private Vector3 rotationAxis1;
    private Vector3 rotationAxis2;
    private Vector3 startPos;

    private float frequency;
    private float phaseOffset;

    void Start()
    {
        startPos = transform.localPosition;

        // Random rotation directions
        rotationAxis1 = Random.onUnitSphere;
        rotationAxis2 = Random.onUnitSphere;

        // Randomized bop frequency and phase
        frequency = Random.Range(minFrequency, maxFrequency);
        phaseOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // Bop motion with randomized frequency and phase
        float yOffset = Mathf.Sin(Time.time * frequency + phaseOffset) * amplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);

        // Rotation
        transform.Rotate(rotationAxis1, rotationSpeed * Time.deltaTime, Space.Self);
        transform.Rotate(rotationAxis2, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
