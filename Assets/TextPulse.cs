using System.Collections;
using TMPro;
using UnityEngine;

public class TextPulse : MonoBehaviour
{
    TMP_Text text;

    [SerializeField]
    float fadeTime = 0.5f;
    void Awake()
    {
        text = GetComponent<TMP_Text>();
        StartCoroutine(Fade());
    }

    private void OnEnable() {
        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        text.CrossFadeAlpha(0, fadeTime, false);
        yield return new WaitForSeconds(fadeTime);
        text.CrossFadeAlpha(1, fadeTime, false);
        yield return new WaitForSeconds(fadeTime);
        StartCoroutine(Fade());
    }
}
