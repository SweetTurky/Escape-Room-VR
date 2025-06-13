using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameStartController : MonoBehaviour
{
    [Header("References")]
    public VRFade fadeController;
    public GameObject xrRig;
    public GameObject continueButton;
    public GameObject lockedFeatureButton;
    public IngredientSpawner ingredientSpawner;

    [Header("Voiceover Clips")]
    public AudioClip[] introClips;
    public AudioClip[] continueClips;
    public AudioClip wormIntroClip;

    private void Start()
    {
        StartCoroutine(GameIntroSequence());
    }

    private IEnumerator GameIntroSequence()
    {
        // 1. Disable movement
        var blocker = xrRig.GetComponentInChildren<IXRMovementBlockable>();
        blocker?.EnableMovement(false);

        // 2. Start black, then fade in
        fadeController.FadeOut(0f); // start fully black
        yield return new WaitForSeconds(0.2f);
        fadeController.FadeIn(2f);
        yield return new WaitForSeconds(2f);

        // 3. Play all intro voice lines sequentially
        foreach (var clip in introClips)
        {
            VoiceoverManager.Instance.QueueVoice(clip, blockMovement: true);
        }

        // 4. Wait until voice queue is done
        yield return new WaitUntil(() => !VoiceoverManager.Instance.IsSpeaking());

        // 5. Show buttons
        continueButton.SetActive(true);
        lockedFeatureButton.SetActive(true);

        /*// Disable the second button visually
        Button lockedBtn = lockedFeatureButton.GetComponent<Button>();
        if (lockedBtn != null)
            lockedBtn.interactable = false;*/
    }

    // Called from the Continue button's OnClick event
    public void OnContinueClicked()
    {
        continueButton.SetActive(false);
        lockedFeatureButton.SetActive(false);

        StartCoroutine(ContinueSequence());
    }

    private IEnumerator ContinueSequence()
    {
        // Step 1: Queue intro voice lines
        foreach (var clip in continueClips)
        {
            VoiceoverManager.Instance.QueueVoice(clip, blockMovement: true);
        }

        // Step 2: Wait until they finish
        yield return new WaitUntil(() => !VoiceoverManager.Instance.IsSpeaking());

        // Step 3: Enable movement
        var blocker = xrRig.GetComponentInChildren<IXRMovementBlockable>();
        blocker?.EnableMovement(true);

        Debug.Log("Game Started!");

        // Step 4: Play worm intro voiceline
        VoiceoverManager.Instance.QueueVoice(wormIntroClip, blockMovement: false);

        // Step 5: Wait 1 second, then spawn ingredient (before voice ends)
        yield return new WaitForSeconds(1f);
        ingredientSpawner.SpawnJarWithIngredient(0); // 0 = Dried Earth Worms
    }
}
