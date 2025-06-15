using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Header("After First Ingredient & Stir")]
    [Tooltip("Voice lines to play once the first ingredient is added AND you've hit the stir-checkpoint.")]
    public AudioClip[] postFirstStepClips;

    [Header("After Second Ingredient & Stir")]
    [Tooltip("Voice lines to play once the second ingredient is added AND you've hit the stir-checkpoint.")]
    public AudioClip[] postSecondStepClips;

    [Header("After 3rd Ingredient & Stir")]
    [Tooltip("Voice lines to play once the third ingredient is added AND you've hit the stir-checkpoint.")]
    public AudioClip[] postThirdStepClips;

    // internal state
    private IXRMovementBlockable _blocker;
    private bool _skipAvailable;

    private void Awake()
    {
        _blocker = xrRig.GetComponentInChildren<IXRMovementBlockable>();
    }

    private void Start()
    {
        // Buttons off until we show them
        continueButton.SetActive(false);
        lockedFeatureButton.SetActive(false);

        StartCoroutine(GameIntroSequence());
    }

    private void Update()
    {
        if (_skipAvailable)
        {
            // poll the right-hand secondary button (B)
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressed) && pressed)
            {
                SkipIntro();
            }
        }
    }

    private IEnumerator GameIntroSequence()
    {
        // 1) Lock movement & turning
        _blocker?.EnableMovement(false);

        // 2) Fade in
        fadeController.FadeOut(0f);
        yield return new WaitForSeconds(0.2f);
        fadeController.FadeIn(2f);
        yield return new WaitForSeconds(2f);

        // 3) Begin queuing intro lines
        foreach (var clip in introClips)
            VoiceoverManager.Instance.QueueVoice(clip, delayAfter: 1f, blockMovement: false);

        // now allow skipping
        _skipAvailable = true;

        // 4) Wait until all lines done OR skip triggered
        yield return new WaitUntil(() =>
            VoiceoverManager.Instance.IsIdle
            || !_skipAvailable
        );

        // show the yes/no choice buttons
        ShowChoiceButtons();
    }

    private void ShowChoiceButtons()
    {
        _skipAvailable = false;

        continueButton.SetActive(true);
        lockedFeatureButton.SetActive(true);
        var lockedBtn = lockedFeatureButton.GetComponent<Button>();
        if (lockedBtn != null)
            lockedBtn.interactable = false;
    }

    private void SkipIntro()
    {
        if (!_skipAvailable) return;
        _skipAvailable = false;

        StopAllCoroutines();                        // kills GameIntroSequence
        VoiceoverManager.Instance.ClearQueue();     // kills the old clip + queue in the VO manager

        ShowChoiceButtons();
    }

    // Called by the Continue button's OnClick
    public void OnContinueClicked()
    {
        continueButton.SetActive(false);
        lockedFeatureButton.SetActive(false);
        StartCoroutine(ContinueSequence());
    }

    private IEnumerator ContinueSequence()
    {
        // 1) Re-enable movement & turning immediately
        _blocker?.EnableMovement(true);

        // 2) Queue continue lines
        foreach (var clip in continueClips)
            VoiceoverManager.Instance.QueueVoice(clip, delayAfter: 1f, blockMovement: false);

        // 3) Wait until they finish
        yield return new WaitUntil(() => VoiceoverManager.Instance.IsIdle);

        Debug.Log("Game Started!");

        // 4) Worm intro + spawn
        VoiceoverManager.Instance.QueueVoice(wormIntroClip, blockMovement: false);
        yield return new WaitForSeconds(1f);
        ingredientSpawner.SpawnJarWithIngredient(0);
    }

    /// <summary>
    /// Hook this up to your CauldronController's OnIngredientAdded+stir-checkpoint event.
    /// </summary>
    public void OnFirstIngredientAndStirComplete()
    {
        StartCoroutine(PostFirstStepSequence());
    }

    public void OnSecondIngredientAndStirComplete()
    {
        StartCoroutine(PostSecondStepSequence());
    }

    public void OnThirdIngredientAndStirComplete()
    {
        StartCoroutine(PostThirdStepSequence());
    }

    private IEnumerator PostFirstStepSequence()
    {
        if (postFirstStepClips == null || postFirstStepClips.Length == 0)
            yield break;

        // 1) Play the first line
        VoiceoverManager.Instance.QueueVoice(
            postFirstStepClips[0],
            delayAfter: 0.5f,
            blockMovement: false
        );

        // 2) Wait until that clip is done
        yield return new WaitUntil(() => VoiceoverManager.Instance.IsIdle);

        // 3) Spawn the next ingredient: FirebloomPetals (index 1)
        ingredientSpawner.SpawnJarWithIngredient(1);

        // 4) Queue the remaining lines
        for (int i = 1; i < postFirstStepClips.Length; i++)
        {
            VoiceoverManager.Instance.QueueVoice(
                postFirstStepClips[i],
                delayAfter: 0.5f,
                blockMovement: false
            );
        }
    }

    private IEnumerator PostSecondStepSequence()
    {
        if (postSecondStepClips == null || postSecondStepClips.Length == 0)
            yield break;

        // 1) Play the first line
        VoiceoverManager.Instance.QueueVoice(
            postSecondStepClips[0],
            delayAfter: 0.5f,
            blockMovement: false
        );

        // 2) Wait until that clip is done
        yield return new WaitUntil(() => VoiceoverManager.Instance.IsIdle);

        // 3) Spawn the next ingredient: FirebloomPetals (index 1)
        ingredientSpawner.SpawnJarWithIngredient(2);

        // 4) Queue the remaining lines
        for (int i = 1; i < postSecondStepClips.Length; i++)
        {
            VoiceoverManager.Instance.QueueVoice(
                postSecondStepClips[i],
                delayAfter: 0.5f,
                blockMovement: false
            );
        }
    }

    private IEnumerator PostThirdStepSequence()
    {
        if (postThirdStepClips == null || postThirdStepClips.Length == 0)
            yield break;

        // 1) Play the first line
        VoiceoverManager.Instance.QueueVoice(
            postThirdStepClips[0],
            delayAfter: 0.5f,
            blockMovement: false
        );

        // 2) Wait until that clip is done
        yield return new WaitUntil(() => VoiceoverManager.Instance.IsIdle);

        // 3) Spawn the next ingredient: FirebloomPetals (index 1)
        //ingredientSpawner.SpawnJarWithIngredient(2);

        /* // 4) Queue the remaining lines
         for (int i = 1; i < postThirdStepClips.Length; i++)
         {
             VoiceoverManager.Instance.QueueVoice(
                 postThirdStepClips[i],
                 delayAfter: 0.5f,
                 blockMovement: false
             );
         }*/

        fadeController.FadeOut(3f);
        yield return new WaitForSeconds(4f);

        // In a standalone build this quits; in the Editor it stops Play mode
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
