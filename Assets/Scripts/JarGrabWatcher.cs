using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class JarGrabWatcher : MonoBehaviour
{
    [Header("Effects")]
    public ParticleEffectController particleController;

    [Header("Return-to-Cauldron Station")]
    [Tooltip("Assign the StationTrigger component (or its GameObject) here; it will be enabled on grab.")]
    public StationTrigger cauldronStationTrigger;

    private XRGrabInteractable _grab;

    // Static flag ensures this is executed only once across all instances
    private static bool _stationActivated = false;

    private void Start()
    {
        _grab = GetComponent<XRGrabInteractable>()
                ?? gameObject.AddComponent<XRGrabInteractable>();

        _grab.selectEntered.AddListener(OnGrabbed);

        if (cauldronStationTrigger != null)
            cauldronStationTrigger.gameObject.SetActive(false);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        particleController?.StopEmission();

        if (!_stationActivated && cauldronStationTrigger != null)
        {
            cauldronStationTrigger.gameObject.SetActive(true);
            _stationActivated = true;
        }
    }

    private void OnDestroy()
    {
        // Reset the static flag if needed (only if you want to reset when jars are destroyed)
        //_stationActivated = false; // Uncomment only if resetting is desired upon destruction
    }
}
