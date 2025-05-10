using UnityEngine;

public class ScalableObject : MonoBehaviour
{
    public float scaleSpeed = 1f;
    public float minScaleFactor = 0.2f;
    public float maxScaleFactor = 2f;

    [HideInInspector] public Vector3 originalScale;
}
