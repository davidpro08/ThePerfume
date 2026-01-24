using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightingEffect : MonoBehaviour
{
    public static LightingEffect instance { get; private set; }
    [SerializeField] private UnityEngine.UI.Image blackPanel;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLighting(float targetAlpha, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }
    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = blackPanel.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            Color c = blackPanel.color;
            c.a = newAlpha;
            blackPanel.color = c;
            yield return null;
        }
        Color finalColor = blackPanel.color;
        finalColor.a = targetAlpha;
        blackPanel.color = finalColor;
    }
}
