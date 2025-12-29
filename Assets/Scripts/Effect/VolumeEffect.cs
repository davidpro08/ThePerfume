using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeEffect : MonoBehaviour
{
    public static VolumeEffect instance { get; private set; }

    public Volume volume;
    Vignette vignette;

    float intensity = 0.45f;
    float Smoothness = 0.75f;
    float duration = 1.0f;
    private Coroutine vignetteCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (volume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.value = 0; // Set initial intensity
            vignette.smoothness.value = 0; // Set initial smoothness
        }
    }

    // Vignette 애니메이션 재생
    public void SetVignette(bool enable)
    {
        if (vignette == null) return;

        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }

        float startIntensity = vignette.intensity.value;
        float startSmoothness = vignette.smoothness.value;
        float endIntensity = enable ? intensity : 0f;
        float endSmoothness = enable ? Smoothness : 0f;

        vignetteCoroutine = StartCoroutine(AnimateVignette(startIntensity, endIntensity, startSmoothness, endSmoothness, duration));
    }

    private IEnumerator AnimateVignette(float startIntensity, float endIntensity, float startSmoothness, float endSmoothness, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            vignette.intensity.value = Mathf.Lerp(startIntensity, endIntensity, t);
            vignette.smoothness.value = Mathf.Lerp(startSmoothness, endSmoothness, t);

            yield return null;
        }

        vignette.intensity.value = endIntensity;
        vignette.smoothness.value = endSmoothness;
    }
}

