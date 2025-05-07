using System.Linq;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    private string expectedShape;
    public ShapeGameManager manager;
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public ParticleSystem successEffect;

    private ShapeGameManager.Hole myHole;

    void Start()
    {
        // Link to my hole reference
        myHole = manager.holes.First(h => h.triggerArea == this);
    }

    public void SetTargetShape(string shape)
    {
        expectedShape = shape;
    }

    private void OnTriggerEnter(Collider other)
    {
        string objectShape = other.tag; // Shape tag

        GameObject droppedObject = other.gameObject;

        // Destroy the shape on any drop
        if (manager.shapeSpawner != null)
        {
            manager.shapeSpawner.RemoveShape(droppedObject);
        }
        else
        {
            Destroy(droppedObject);
        }

        if (objectShape == expectedShape)
        {
            if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);
            if (successEffect) successEffect.Play();

            manager.HoleCorrect(myHole);
        }
        else
        {
            if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
            manager.HoleIncorrect(myHole);
        }
    }
}

