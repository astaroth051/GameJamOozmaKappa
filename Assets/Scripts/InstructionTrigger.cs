using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private static bool algunTriggerActivo = false;
    private bool esSegundoNivel = false;
    private bool esTercerNivel = false;

    private void Start()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        esSegundoNivel = escenaActual == "SegundoNivel";
        esTercerNivel = escenaActual == "TercerNivel";

        if (fuenteAudio == null)
            fuenteAudio = gameObject.AddComponent<AudioSource>();

        fuenteAudio.spatialBlend = 0f;
        fuenteAudio.volume = 1f;
        fuenteAudio.priority = 0;
        fuenteAudio.loop = false;
        fuenteAudio.playOnAwake = false;
        fuenteAudio.reverbZoneMix = 0f;

        if (textoUI != null)
            textoUI.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !gameObject.CompareTag("Pasos") || yaActivado)
            return;

        Debug.Log($"[InstructionTrigger] Jugador tocó el trigger: {gameObject.name}");

        if (algunTriggerActivo)
        {
            Debug.Log($"[InstructionTrigger] Otro trigger activo, esperando turno: {gameObject.name}");
            StartCoroutine(EsperarYReproducir());
            return;
        }

        yaActivado = true;
        rutina = StartCoroutine(MostrarInstrucciones());
    }

    private IEnumerator EsperarYReproducir()
    {
        Debug.Log($"[InstructionTrigger] {gameObject.name} está esperando su turno...");

        yield return new WaitWhile(() => algunTriggerActivo);

        if (yaActivado) yield break;

        yaActivado = true;
        rutina = StartCoroutine(MostrarInstrucciones());
    }

    private IEnumerator MostrarInstrucciones()
    {
        ejecutando = true;
        algunTriggerActivo = true;

        Debug.Log($"[InstructionTrigger] → Iniciando secuencia: {gameObject.name}");

        foreach (var paso in pasos)
        {
            if (textoUI != null)
                textoUI.text = paso.texto;

            if (paso.audios != null && paso.audios.Count > 0)
            {
                foreach (var clip in paso.audios)
                {
                    if (clip == null) continue;

                    GameObject tempAudio = new GameObject($"TempAudio_{clip.name}");
                    AudioSource temp = tempAudio.AddComponent<AudioSource>();
                    temp.spatialBlend = 0f;

                    // Volumen según nivel actual
                    if (esTercerNivel)
                        temp.volume = 5f; // volumen doble en TercerNivel
                    else if (esSegundoNivel)
                        temp.volume = 3f;
                    else
                        temp.volume = 1f;

                    temp.priority = 0;
                    temp.loop = false;
                    temp.playOnAwake = false;
                    temp.clip = clip;
                    temp.Play();

                    Debug.Log($"[InstructionTrigger] Reproduciendo audio: {clip.name}");
                    Destroy(tempAudio, clip.length + 0.2f);
                    yield return new WaitForSeconds(clip.length + 0.1f);
                }
            }

            yield return new WaitForSeconds(duracionPorPaso);
        }

        if (textoUI != null)
            textoUI.text = "";

        Debug.Log($"[InstructionTrigger] Secuencia terminada en {gameObject.name}");

        ejecutando = false;
        algunTriggerActivo = false;
        yaActivado = true;
    }

    private void OnDestroy()
    {
        if (algunTriggerActivo && ejecutando)
        {
            Debug.LogWarning($"[InstructionTrigger] {gameObject.name} fue destruido mientras ejecutaba. Liberando bloqueo.");
            algunTriggerActivo = false;
        }
    }
}
