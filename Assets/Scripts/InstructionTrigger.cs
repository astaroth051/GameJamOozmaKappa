using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class InstructionStep
{
    [TextArea(2, 4)]
    public string texto;
    public List<AudioClip> audios;
}

public class InstructionTrigger : MonoBehaviour
{
    [Header("Configuración general")]
    public List<InstructionStep> pasos;
    public float duracionPorPaso = 4f;

    [Header("Referencias UI y Audio")]
    public TextMeshProUGUI textoUI;
    public AudioSource fuenteAudio;

    private bool ejecutando = false;
    private bool yaActivado = false;
    private Coroutine rutina;

    // 🔒 Solo un trigger activo a la vez
    private static bool algunTriggerActivo = false;

    private void Start()
    {
        if (fuenteAudio == null)
        {
            fuenteAudio = gameObject.AddComponent<AudioSource>();
            fuenteAudio.spatialBlend = 0f; // 2D (no afectado por distancia)
            fuenteAudio.volume = 1f;
            fuenteAudio.priority = 0;      // máxima prioridad
        }

        if (textoUI != null)
            textoUI.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo el jugador y solo si no se activó antes
        if (!other.CompareTag("Player") || !gameObject.CompareTag("Pasos") || yaActivado)
            return;

        // Si otro trigger está activo, espera su turno
        if (algunTriggerActivo)
        {
            StartCoroutine(EsperarYReproducir());
            return;
        }

        yaActivado = true;
        rutina = StartCoroutine(MostrarInstrucciones());
    }

    private IEnumerator EsperarYReproducir()
    {
        yield return new WaitWhile(() => algunTriggerActivo);

        // Si ya se activó mientras esperaba, no se repite
        if (yaActivado) yield break;

        yaActivado = true;
        rutina = StartCoroutine(MostrarInstrucciones());
    }

    private IEnumerator MostrarInstrucciones()
    {
        ejecutando = true;
        algunTriggerActivo = true;

        foreach (var paso in pasos)
        {
            if (textoUI != null)
                textoUI.text = paso.texto;

            // Reproduce los audios en orden
            if (paso.audios != null && paso.audios.Count > 0)
            {
                foreach (var clip in paso.audios)
                {
                    if (clip == null) continue;

                    // Usa un AudioSource temporal para no interrumpir otros sonidos
                    GameObject tempAudio = new GameObject("TempInstructionAudio");
                    AudioSource temp = tempAudio.AddComponent<AudioSource>();
                    temp.clip = clip;
                    temp.spatialBlend = 0f;
                    temp.volume = 1f;
                    temp.priority = 0; // prioridad más alta
                    temp.Play();

                    Destroy(tempAudio, clip.length + 0.2f);
                    yield return new WaitForSeconds(clip.length + 0.1f);
                }
            }

            yield return new WaitForSeconds(duracionPorPaso);
        }

        if (textoUI != null)
            textoUI.text = "";

        ejecutando = false;
        algunTriggerActivo = false;

        //  Evita que se vuelva a reproducir nunca más
        yaActivado = true;
    }
}
