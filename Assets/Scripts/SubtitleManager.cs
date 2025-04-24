using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public Text subtitleText;        // Hook up a UI Text or TMP Text in the Inspector
    public float displayTime = 3f;   // Default time to show each caption

    private Coroutine _current;

    public void ShowSubtitle(string message, float duration = -1f)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(DoShow(message, duration > 0 ? duration : displayTime));
    }

    private IEnumerator DoShow(string msg, float time)
    {
        subtitleText.text = msg;
        subtitleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        subtitleText.gameObject.SetActive(false);
    }
}
