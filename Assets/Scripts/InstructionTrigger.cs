using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class InstructionStep
{
    [TextArea(2, 4)]
    public string texto;               // Texto a mostrar
    public List<AudioClip> audios;     // Lista de audios (ahora puede haber varios)
}

public class InstructionTrigger : MonoBehaviour
{
    [Header("Configuración general")]
    public List<InstructionStep> pasos;     // Lista de instrucciones
    public float duracionPorPaso = 4f;      // Duración del texto (además del audio)
    public bool repetir = false;

    [Header("Referencias UI y Audio")]
    public TextMeshProUGUI textoUI;
    public AudioSource fuenteAudio;

    private bool ejecutando = false;
    private bool yaActivado = false;
    private Coroutine rutina;

    private void Start()
    {
       

        if (fuenteAudio == null)
        {
            fuenteAudio = gameObject.AddComponent<AudioSource>();
            fuenteAudio.spatialBlend = 0f; // 2D por defecto
           
        }

        if (textoUI != null)
        {
            textoUI.text = "";
          
        }
        else
        {
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       

        if (other.CompareTag("Player") && gameObject.CompareTag("Pasos") && !yaActivado)
        {
            

            yaActivado = true;

            if (ejecutando && rutina != null)
            {
                StopCoroutine(rutina);
               
            }

            rutina = StartCoroutine(MostrarInstrucciones());
        }
        else
        {
            if (!gameObject.CompareTag("Pasos"))
            if (!other.CompareTag("Player"))
            if (yaActivado)
                Debug.Log("[InstructionTrigger] Ya se activó antes, no volverá a ejecutarse.");
        }
    }

    IEnumerator MostrarInstrucciones()
    {
        ejecutando = true;
      

        foreach (var paso in pasos)
        {
            if (textoUI != null)
                textoUI.text = paso.texto;

      

            // Reproducir múltiples audios en orden
            if (paso.audios != null && paso.audios.Count > 0)
            {
               
                foreach (var clip in paso.audios)
                {
                    if (clip == null) continue;

                    fuenteAudio.clip = clip;
                    fuenteAudio.Play();
                   

                    yield return new WaitForSeconds(clip.length + 0.1f); // pequeña pausa entre clips
                }
            }

            yield return new WaitForSeconds(duracionPorPaso);
        }

        if (textoUI != null)
            textoUI.text = "";

        ejecutando = false;
 

        if (repetir)
        {
            yaActivado = false;
            StartCoroutine(MostrarInstrucciones());
        }
    }
}
