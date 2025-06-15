using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CauldronEffects : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your CauldronController here")]
    public CauldronController cauldron;

    [Header("Ingredient Particle Effects")]
    public ParticleSystem wormParticles;
    public ParticleSystem firebloomParticles;
    public ParticleSystem dragonToothParticles;

    [Header("Ingredient SFX")]
    public AudioClip wormSfx;
    public AudioClip firebloomSfx;
    public AudioClip dragonToothSfx;

    [Header("Ingredient Voice Lines (can be multiple)")]
    public AudioClip[] wormVoiceLines;
    public AudioClip[] firebloomVoiceLines;
    public AudioClip[] dragonToothVoiceLines;

    [Tooltip("Pause after each VO clip")]
    public float voiceDelayAfter = 0.5f;

    [Header("Potion Finished Effect")]
    public ParticleSystem potionFinishedParticles;
    public AudioClip potionFinishedSfx;
    public AudioClip[] potionFinishedVoiceLines;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        //cauldron.OnIngredientAdded.AddListener(HandleIngredient);
        //cauldron.OnPotionFinished.AddListener(HandlePotionFinished);
    }

    private void OnDisable()
    {
        cauldron.OnIngredientAdded.RemoveListener(HandleIngredient);
        cauldron.OnPotionFinished.RemoveListener(HandlePotionFinished);
    }

    public void HandleIngredient(IngredientType type)
    {
        Debug.Log($"HandleIngredient triggered for {type}", this);

        switch (type)
        {
            case IngredientType.DriedWorm:
                PlayEffect(wormParticles, wormSfx);
                QueueVoiceLines(wormVoiceLines);
                break;
            case IngredientType.FirebloomPetals:
                PlayEffect(firebloomParticles, firebloomSfx);
                QueueVoiceLines(firebloomVoiceLines);
                break;
            case IngredientType.DragonsTooth:
                PlayEffect(dragonToothParticles, dragonToothSfx);
                QueueVoiceLines(dragonToothVoiceLines);
                break;
            default:
                Debug.LogWarning($"No effect or VO configured for {type}");
                break;
        }
    }

    public void HandlePotionFinished()
    {
        PlayEffect(potionFinishedParticles, potionFinishedSfx);
        //QueueVoiceLines(potionFinishedVoiceLines);
    }

    private void PlayEffect(ParticleSystem ps, AudioClip clip)
    {
        if (ps != null) ps.Play();
        if (clip != null) _audioSource.PlayOneShot(clip, 0.5f);
    }

    private void QueueVoiceLines(AudioClip[] clips)
    {
        if (clips == null) return;
        foreach (var clip in clips)
        {
            if (clip == null) continue;
            VoiceoverManager.Instance.QueueVoice(
                clip,
                delayAfter: voiceDelayAfter,
                blockMovement: false
            );
        }
    }
}
