using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeAndTrigger : MonoBehaviour
{
    [Header("Fade Settings")]
    public Renderer fadeMaterialRenderer; 
    public float fadeDuration = 2f;

    [Header("Audio")]
    public List<AudioClip> clipsSimultaneos;
    public float volumen = 1f;

    [Header("Objetos a desactivar")]
    public List<GameObject> objetosADesactivar;

    [Header("UI a ocultar durante el fade")]
    public List<TextMeshProUGUI> textosTMP;
    public List<Scrollbar> scrollBars;

    [Header("Script a activar (puerta)")]
    public DoorInteractable puertaScript; 

    private bool activado = false;
    private List<AudioSource> audiosActivos = new List<AudioSource>();

    private void Start()
    {
        if (fadeMaterialRenderer != null)
        {
            Color c = fadeMaterialRenderer.material.color;
            c.a = 0f; 
            fadeMaterialRenderer.material.color = c;
        }

        if (puertaScript != null)
            puertaScript.interactuable = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (!other.CompareTag("Player")) return;

        activado = true;
        Debug.Log("[FadeAndTrigger] Jugador activó el trigger.");
        StartCoroutine(ActivarSecuencia());
    }

    private IEnumerator ActivarSecuencia()
    {
        // 🔹 Ocultar todos los elementos de UI
        OcultarUI(true);

        // FADE IN (pantalla negra)
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 🔹 Esperar a que terminen los audios de instrucciones
        Debug.Log("[FadeAndTrigger] Esperando a que terminen audios de instrucciones...");
        yield return StartCoroutine(EsperarInstructionAudios());

        // 🔹 Reproducir audios propios del FadeAndTrigger
        if (clipsSimultaneos != null && clipsSimultaneos.Count > 0)
        {
            Debug.Log("[FadeAndTrigger] Reproduciendo audios del FadeAndTrigger...");
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
                Debug.Log($"[FadeAndTrigger] Audio reproducido: {clip.name}");
            }
        }

        // 🔹 Esperar que terminen los audios del FadeAndTrigger
        yield return StartCoroutine(EsperarAudiosActivos());

        // 🔹 Desactivar objetos una vez terminados los sonidos
        foreach (var obj in objetosADesactivar)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"[FadeAndTrigger] Objeto desactivado: {obj.name}");
            }
        }

        // FADE OUT (volver a ver la escena)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        // 🔹 Restaurar visibilidad de la UI
        OcultarUI(false);

        // Activar puerta al final del fade
        if (puertaScript != null)
        {
            puertaScript.interactuable = true;
            if (puertaScript.canvasInteraccion != null)
                puertaScript.canvasInteraccion.gameObject.SetActive(true);

            Debug.Log("[FadeAndTrigger] Puerta lista para interactuar (post-fade).");
        }

        Debug.Log("[FadeAndTrigger] Secuencia completa.");
    }

    private void OcultarUI(bool ocultar)
    {
        if (textosTMP != null)
        {
            foreach (var texto in textosTMP)
            {
                if (texto != null)
                    texto.enabled = !ocultar;
            }
        }

        if (scrollBars != null)
        {
            foreach (var sb in scrollBars)
            {
                if (sb != null)
                    sb.gameObject.SetActive(!ocultar);
            }
        }

        Debug.Log($"[FadeAndTrigger] UI {(ocultar ? "ocultada" : "restaurada")}.");
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeMaterialRenderer == null)
        {
            Debug.LogWarning("[FadeAndTrigger] No se asignó el material de fade.");
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
