using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OpenDoor(GameObject door)
    {
        if (door != null)
        {
            Animator animator = door.GetComponent<Animator>();

            if (animator != null)
            {
                animator.SetTrigger("Open"); // Trigger animation
            }
            else
            {
                Debug.LogWarning($"No Animator found on {door.name}, disabling instead.");
                door.SetActive(false); // Fallback if no animation
            }
        }
    }

    public void DisplayFeedback(string message)
    {
        Debug.Log($"[Feedback] {message}");
        // Optionally trigger UI or haptics here
    }
}
