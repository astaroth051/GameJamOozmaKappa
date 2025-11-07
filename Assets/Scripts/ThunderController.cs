using UnityEngine;
using System.Collections;

public class ThunderController : MonoBehaviour
{
    [Header("Luz de relámpago")]
    public Light lightningLight;            // Luz asignada (Directional, Spot o Point)
    public float maxIntensity = 10f;        // Intensidad máxima del relámpago
    public float flashDuration = 0.15f;     // Duración del destello

    [Header("Sonidos de trueno")]
    public AudioClip[] thunderSounds;       // Tres sonidos distintos
    public AudioSource thunderAudioSource;  // Asigna tu propio AudioSource en el Inspector

    [Header("Rango de tiempo entre truenos (segundos)")]
    public float minDelay = 15f;
    public float maxDelay = 25f;

    private void Start()
    {
        // Verificación de componentes
        if (lightningLight == null)
        {
            Debug.LogWarning(" No se ha asignado ninguna luz al ThunderController.");
        }

        if (thunderAudioSource == null)
        {
            Debug.LogWarning(" No se ha asignado ningún AudioSource. Se agregará uno automáticamente.");
            thunderAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Lanza un primer trueno rápido y luego continúa con el ciclo normal
        StartCoroutine(InitialThunder());
    }

    private IEnumerator InitialThunder()
    {
        // Espera exactamente 3 segundos antes del primer trueno
        yield return new WaitForSeconds(3f);

        // Primer relámpago + trueno
        StartCoroutine(LightFlash());
        yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
        PlayRandomThunder();

        // Luego entra al ciclo regular
        StartCoroutine(ThunderRoutine());
    }

    private IEnumerator ThunderRoutine()
    {
        while (true)
        {
            // Espera aleatoria entre truenos
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            // Relámpago
            StartCoroutine(LightFlash());

            // Trueno con retraso
            yield return new WaitForSeconds(Random.Range(0.6f, 1.4f));
            PlayRandomThunder();
        }
    }

    private void PlayRandomThunder()
    {
        if (thunderSounds.Length == 0 || thunderAudioSource == null) return;
        int index = Random.Range(0, thunderSounds.Length);
        thunderAudioSource.PlayOneShot(thunderSounds[index]);
    }

    private IEnumerator LightFlash()
    {
        if (lightningLight == null) yield break;

        // Primer flash
        lightningLight.intensity = maxIntensity;
        lightningLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        lightningLight.enabled = false;

        // Segundo flash corto (efecto natural)
        if (Random.value > 0.6f)
        {
            yield return new WaitForSeconds(0.05f);
            lightningLight.intensity = maxIntensity * Random.Range(0.5f, 1f);
            lightningLight.enabled = true;
            yield return new WaitForSeconds(flashDuration * 0.5f);
            lightningLight.enabled = false;
        }

        // Restaurar intensidad a 0
        lightningLight.intensity = 0f;
    }
}
