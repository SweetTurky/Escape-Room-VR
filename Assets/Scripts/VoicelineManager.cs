using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct VoicelineConfig
{
    [Tooltip("Which room index this config applies to (0 = first room)")]
    public int roomIndex;
    public AudioClip[] introClips;
    public AudioClip[] hintClips;
    public float hintDelay;
    public bool useGlitchSnapshot;
}

public class VoicelineManager : MonoBehaviour
{
    [Tooltip("Configure intro + hint clips per room index")]
    public VoicelineConfig[] configs;
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot glitchSnapshot;
    public float glitchTransitionTime = 0.2f;
    public SubtitleManager subtitleManager;

    Dictionary<int, VoicelineConfig> _configMap;
    AudioSource _audio;
    Coroutine _hintRoutine;
    int _currentRoomIndex;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _configMap = new Dictionary<int, VoicelineConfig>();
        foreach (var cfg in configs)
            _configMap[cfg.roomIndex] = cfg;
    }

    void OnEnable()
    {
        GameStateManager.Instance.OnStateChanged += HandleRoomChanged;
    }

    void OnDisable()
    {
        GameStateManager.Instance.OnStateChanged -= HandleRoomChanged;
    }

    void HandleRoomChanged(int newRoomIndex)
    {
        _currentRoomIndex = newRoomIndex;
        if (_hintRoutine != null) StopCoroutine(_hintRoutine);
        if (_configMap.TryGetValue(newRoomIndex, out var cfg))
            StartCoroutine(PlayIntroAndHints(cfg));
    }

    IEnumerator PlayIntroAndHints(VoicelineConfig cfg)
    {
        // intros
        foreach (var clip in cfg.introClips)
        {
            PlayClipWithExtras(clip, isHint: false, cfg.useGlitchSnapshot);
            yield return new WaitForSeconds(clip.length + 0.2f);
        }
        // hints
        _hintRoutine = StartCoroutine(HintLoop(cfg));
    }

    IEnumerator HintLoop(VoicelineConfig cfg)
    {
        yield return new WaitForSeconds(cfg.hintDelay);
        while (_currentRoomIndex == cfg.roomIndex)
        {
            foreach (var hint in cfg.hintClips)
            {
                PlayClipWithExtras(hint, isHint: true, cfg.useGlitchSnapshot);
                yield return new WaitForSeconds(hint.length + cfg.hintDelay);
                if (_currentRoomIndex != cfg.roomIndex) yield break;
            }
        }
    }

    void PlayClipWithExtras(AudioClip clip, bool isHint, bool useGlitch)
    {
        // modulation
        if (useGlitch && glitchSnapshot != null)
            glitchSnapshot.TransitionTo(glitchTransitionTime);
        else
            normalSnapshot?.TransitionTo(glitchTransitionTime);

        // play audio
        _audio.PlayOneShot(clip);

        // subtitles
        subtitleManager?.ShowSubtitle(clip.name, clip.length);

        // analytics
        AnalyticsManager.LogEvent("voiceline_played", new Dictionary<string, object>{
            {"roomIndex", _currentRoomIndex},
            {"clip", clip.name},
            {"hint", isHint ? 1 : 0}
        });

        // restore normal audio after clip
        StartCoroutine(ResetSnapshotAfter(clip.length + 0.1f));
    }

    IEnumerator ResetSnapshotAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        normalSnapshot?.TransitionTo(glitchTransitionTime);
    }

    public void RequestHint()
    {
        if (_configMap.TryGetValue(_currentRoomIndex, out var cfg) && cfg.hintClips.Length > 0)
        {
            if (_hintRoutine != null) StopCoroutine(_hintRoutine);
            PlayClipWithExtras(cfg.hintClips[0], isHint: true, cfg.useGlitchSnapshot);
            AnalyticsManager.LogEvent("hint_requested_manually", new Dictionary<string, object>{
                {"roomIndex", _currentRoomIndex}
            });
            _hintRoutine = StartCoroutine(HintLoop(cfg));
        }
    }
}
