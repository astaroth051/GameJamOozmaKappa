using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeSceneChange : MonoBehaviour
{
    [Header("Fade Settings")]
    public Renderer fadeMaterialRenderer; // Plano negro (con material que tenga canal Alpha)
    public float fadeDuration = 2f;

    [Header("Audio Opcional (simultáneo antes del cambio)")]
    public List<AudioClip> clipsSimultaneos;
    public float volumen = 1f;

    [Header("Nombre de la escena destino")]
    public string nombreEscenaDestino = "IntroNivel3";

    private bool activado = false;
    private List<AudioSource> audiosActivos = new List<AudioSource>();

    private void Start()
    {
        // Asegura que el fade inicia completamente transparente
        if (fadeMaterialRenderer != null)
        {
            Color c = fadeMaterialRenderer.material.color;
            c.a = 0f;
            fadeMaterialRenderer.material.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (!other.CompareTag("Player")) return;

        activado = true;
        Debug.Log($"[FadeSceneChange] Jugador activó el cambio de escena hacia {nombreEscenaDestino}");
        StartCoroutine(ActivarCambioDeEscena());
    }

    private IEnumerator ActivarCambioDeEscena()
    {
        // 🔹 Esperar que terminen los audios de instrucciones antes del fade
        yield return StartCoroutine(EsperarInstructionAudios());

        // 🔹 Fade a negro (oculta pantalla)
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 🔹 Reproducir audios opcionales del trigger
        if (clipsSimultaneos != null && clipsSimultaneos.Count > 0)
        {
            Debug.Log("[FadeSceneChange] Reproduciendo audios simultáneos antes del cambio...");
            foreach (var clip in clipsSimultaneos)
            {
                if (clip == null) continue;

                AudioSource temp = new GameObject($"TempAudio_{clip.name}").AddComponent<AudioSource>();
                temp.spatialBlend = 0f;
                temp.volume = volumen;
                temp.priority = 0;
                temp.clip = clip;
                temp.Play();
                audiosActivos.Add(temp);
                Destroy(temp.gameObject, clip.length + 0.25f);
            }
        }

        // 🔹 Esperar a que terminen los audios propios
        yield return StartCoroutine(EsperarAudiosActivos());

        Debug.Log("[FadeSceneChange] Todos los audios han terminado. Cambiando de escena...");

        // 🔹 Cargar la nueva escena
        SceneManager.LoadScene(nombreEscenaDestino);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeMaterialRenderer == null)
        {
            Debug.LogWarning("[FadeSceneChange] No se asignó el material de fade.");
            yield break;
        }

        float t = 0f;
        Color c = fadeMaterialRenderer.material.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            fadeMaterialRenderer.material.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeMaterialRenderer.material.color = c;
    }

    private IEnumerator EsperarInstructionAudios()
    {
        while (HayInstructionAudiosActivos())
            yield return null;
    }

    private IEnumerator EsperarAudiosActivos()
    {
        while (HayAudiosReproduciendose())
            yield return null;
    }

    private bool HayAudiosReproduciendose()
    {
        audiosActivos.RemoveAll(a => a == null);
        foreach (var audio in audiosActivos)
        {
            if (audio != null && audio.isPlaying)
                return true;
        }
        return false;
    }

    private bool HayInstructionAudiosActivos()
    {
        InstructionTrigger[] triggers = FindObjectsOfType<InstructionTrigger>();
        foreach (var t in triggers)
        {
            AudioSource[] audios = t.GetComponentsInChildren<AudioSource>();
            foreach (var a in audios)
            {
                if (a != null && a.isPlaying)
                    return true;
            }
        }
        return false;
    }
}
