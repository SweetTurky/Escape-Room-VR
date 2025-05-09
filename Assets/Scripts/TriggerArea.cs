using System.Linq;
using UnityEngine;
using System.Collections;

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

    private IEnumerator PlayAndStopEffect(ParticleSystem effect)
    {
        effect.Play();
        yield return new WaitForSeconds(1.5f);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnTriggerEnter(Collider other)
    {
        string objectShape = other.tag; // The tag on the dropped object
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
            if (successEffect) StartCoroutine(PlayAndStopEffect(successEffect));

            manager.HoleCorrect(myHole);
        }
        else
        {
            if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
            manager.HoleIncorrect(myHole, objectShape); // Respawn just this shape
        }
    }
}
