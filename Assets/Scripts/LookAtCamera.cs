using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform targetCamera;

    void Start()
    {
        // If no camera assigned, default to main camera
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            // Face the camera but keep upright
            Vector3 lookDirection = transform.position - targetCamera.position;
            lookDirection.y = 0f; // keep it upright (no tilting)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
