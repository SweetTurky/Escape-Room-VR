using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CauldronEffects : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your CauldronController here")]
    public CauldronController cauldron;

    [Header("Ingredient Particle Effects")]
    public ParticleSystem wormParticles;          // e.g. for DriedEarthWorms
    public ParticleSystem firebloomParticles;     // e.g. for FirebloomPetals
    public ParticleSystem dragonToothParticles;   // for DragonsTooth

    [Header("Ingredient Audio Clips")]
    public AudioClip wormSfx;
    public AudioClip firebloomSfx;
    public AudioClip dragonToothSfx;

    [Header("Potion Finished Effect")]
    public ParticleSystem potionFinishedParticles;
    public AudioClip potionFinishedSfx;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (cauldron != null)
        {
            cauldron.OnIngredientAdded.AddListener(HandleIngredient);
            cauldron.OnPotionFinished.AddListener(HandlePotionFinished);
        }
    }

    private void OnDisable()
    {
        if (cauldron != null)
        {
            cauldron.OnIngredientAdded.RemoveListener(HandleIngredient);
            cauldron.OnPotionFinished.RemoveListener(HandlePotionFinished);
        }
    }

    /// <summary>
    /// Called whenever the CauldronController invokes OnIngredientAdded.
    /// Plays a different effect based on the ingredient type.
    /// </summary>
    public void HandleIngredient(IngredientType type)
    {
        switch (type)
        {
            case IngredientType.DriedWorm:
                PlayEffect(wormParticles, wormSfx);
                break;

            case IngredientType.FirebloomPetals:
                PlayEffect(firebloomParticles, firebloomSfx);
                break;

            case IngredientType.DragonsTooth:
                PlayEffect(dragonToothParticles, dragonToothSfx);
                break;

            default:
                Debug.LogWarning($"No effect configured for ingredient: {type}");
                break;
        }
    }

    /// <summary>
    /// Called when the potion is fully brewed.
    /// </summary>
    public void HandlePotionFinished()
    {
        PlayEffect(potionFinishedParticles, potionFinishedSfx);
    }

    /// <summary>
    /// Helper to play a particle system and an SFX.
    /// </summary>
    private void PlayEffect(ParticleSystem ps, AudioClip clip)
    {
        if (ps != null)
            ps.Play();

        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }
}
