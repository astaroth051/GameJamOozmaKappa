using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessController : MonoBehaviour
{
    private Volume volume;
    private Bloom bloom;
    private ChromaticAberration chroma;
    private ColorAdjustments colorAdjust;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out chroma);
        volume.profile.TryGet(out colorAdjust);
    }

    // --- Efecto de píldora ---
    public void PillEffect()
    {
        StopAllCoroutines();
        StartCoroutine(PillRoutine());
    }

    private IEnumerator PillRoutine()
    {
        float time = 0f;
        float bloomDuration = 5f;
        float chromaDuration = 5f;
        float colorFadeDuration = 15f;

        // Animar aumento
        while (time < bloomDuration)
        {
            time += Time.deltaTime;
            float t = time / bloomDuration;
            if (bloom != null) bloom.intensity.value = Mathf.Lerp(0f, 10f, t);
            if (chroma != null) chroma.intensity.value = Mathf.Lerp(0f, 0.25f, t);
            if (colorAdjust != null) colorAdjust.saturation.value = Mathf.Lerp(0f, -50f, t);
            yield return null;
        }

        // Mantener pico por un momento
        yield return new WaitForSeconds(1f);

        // Desvanecer gradualmente
        time = 0f;
        while (time < colorFadeDuration)
        {
            time += Time.deltaTime;
            float t = time / colorFadeDuration;
            if (bloom != null) bloom.intensity.value = Mathf.Lerp(10f, 0f, t);
            if (chroma != null) chroma.intensity.value = Mathf.Lerp(0.25f, 0f, t);
            if (colorAdjust != null) colorAdjust.saturation.value = Mathf.Lerp(-50f, 0f, t);
            yield return null;
        }

        // Asegurar retorno a normalidad
        if (bloom != null) bloom.intensity.value = 0f;
        if (chroma != null) chroma.intensity.value = 0f;
        if (colorAdjust != null) colorAdjust.saturation.value = 0f;
    }
}
