using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class JarGrabWatcher : MonoBehaviour
{
    public ParticleEffectController particleController;

    private void Start()
    {
        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
        if (grab == null)
        {
            grab = gameObject.AddComponent<XRGrabInteractable>();
        }

        grab.selectEntered.AddListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (particleController != null)
        {
            particleController.StopEmission();
        }
    }
}
