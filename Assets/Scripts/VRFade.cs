using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRFade : MonoBehaviour
{
    public Image fadeImage; // assign the UI Image component
    private Coroutine currentFade;

    private void Awake()
    {
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();

        // Start fully black or fully transparent as you prefer
        SetAlpha(1f);
    }

    public void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(alpha);
        fadeImage.color = c;
    }

    public void FadeTo(float targetAlpha, float duration)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeCoroutine(targetAlpha, duration));
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(targetAlpha);
        currentFade = null;
    }

    // Convenience methods
    public void FadeIn(float duration) => FadeTo(0f, duration);   // Fade to fully transparent
    public void FadeOut(float duration) => FadeTo(1f, duration);  // Fade to fully black
}
