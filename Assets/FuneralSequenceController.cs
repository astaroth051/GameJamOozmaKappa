using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class FuneralSequenceController : MonoBehaviour
{
    [Header("Referencias principales")]
    public GameObject ellis;
    public AudioClip narrationClip;         
    public Camera mainCamera;
    public Transform focusPoint;
    public Renderer fadeRenderer;
    public string nextSceneName = "IntroNivel5";

    [Header("Colliders de activación")]
    public Collider firstTrigger;
    public Collider secondTrigger;

    [Header("Transiciones")]
    public float cameraFocusTime = 2.5f;
    public float fadeToBlackTime = 2f;
    public float fadeToWhiteTime = 3f;
    public Color fadeToBlackColor = Color.black;
    public Color fadeToWhiteColor = Color.white;

    private bool firstActivated = false;
    private bool secondActivated = false;
    private bool sequenceRunning = false;
    private MonoBehaviour ellisController;
    private Material fadeMaterial;
    private AudioSource narrationSource;

    private void Start()
    {
        if (ellis != null)
        {
            ellisController = ellis.GetComponent<MonoBehaviour>();
            Debug.Log("[FuneralSequence] Ellis detectado correctamente.");
        }
        else
        {
            Debug.LogWarning("[FuneralSequence] No se asignó el objeto Ellis.");
        }

        // Crear automáticamente el AudioSource si hay un clip
        if (narrationClip != null)
        {
            narrationSource = gameObject.AddComponent<AudioSource>();
            narrationSource.clip = narrationClip;
            narrationSource.playOnAwake = false;
            narrationSource.spatialBlend = 0f;
            narrationSource.volume = 1f;
            Debug.Log("[FuneralSequence] AudioSource creado automáticamente con clip: " + narrationClip.name);
        }
        else
        {
            Debug.LogWarning("[FuneralSequence] No se asignó ningún clip de narración.");
        }

        // Preparar material de fade
        if (fadeRenderer != null)
        {
            fadeMaterial = fadeRenderer.material;
            fadeMaterial.color = new Color(1, 1, 1, 0);
            fadeMaterial.SetColor("_EmissionColor", Color.black);
            Debug.Log("[FuneralSequence] Material de fade inicializado en transparente.");
        }
    }

    // Detecta entrada a los triggers
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Debug.Log("[FuneralSequence] Se detectó colisión con " + other.name + " pero no tiene el tag 'Player'. Ignorado.");
            return;
        }

        Debug.Log("[FuneralSequence] El jugador (" + other.name + ") entró a un trigger: " + other.gameObject.name);

        if (firstTrigger != null && other.bounds.Intersects(firstTrigger.bounds))
        {
            Debug.Log("[FuneralSequence] Colisión confirmada con PRIMER trigger.");
            OnZoneEntered(true);
        }
        else if (secondTrigger != null && other.bounds.Intersects(secondTrigger.bounds))
        {
            Debug.Log("[FuneralSequence] Colisión confirmada con SEGUNDO trigger.");
            OnZoneEntered(false);
        }
        else
        {
            Debug.Log("[FuneralSequence] El collider no coincide con los triggers definidos.");
        }
    }

    public void OnZoneEntered(bool isFirstZone)
    {
        if (isFirstZone)
        {
            if (!firstActivated)
            {
                Debug.Log("[FuneralSequence] Primer trigger confirmado. Reproduciendo audio...");
                StartCoroutine(PlayFirstAudio());
                firstActivated = true;
            }
            else Debug.Log("[FuneralSequence] Primer trigger ya fue activado antes.");
        }
        else
        {
            if (!secondActivated)
            {
                Debug.Log("[FuneralSequence] Segundo trigger confirmado. Iniciando secuencia final...");
                StartCoroutine(FinalSequence());
                secondActivated = true;
            }
            else Debug.Log("[FuneralSequence] Segundo trigger ya fue activado antes.");
        }
    }

    private IEnumerator PlayFirstAudio()
    {
        if (narrationSource == null || narrationSource.clip == null)
        {
            Debug.LogWarning("[FuneralSequence] No hay clip de narración asignado.");
            yield break;
        }

        if (!narrationSource.isPlaying)
        {
            narrationSource.Play();
            Debug.Log("[FuneralSequence] Reproduciendo audio: " + narrationSource.clip.name);
        }
        else
        {
            Debug.Log("[FuneralSequence] Audio ya estaba reproduciéndose.");
        }
        yield return null;
    }

    private IEnumerator FinalSequence()
    {
        if (sequenceRunning)
        {
            Debug.Log("[FuneralSequence] Secuencia final ya en curso, ignorando duplicado.");
            yield break;
        }

        sequenceRunning = true;
        Debug.Log("[FuneralSequence] Iniciando secuencia final.");

        if (ellisController != null)
        {
            ellisController.enabled = false;
            Debug.Log("[FuneralSequence] Movimiento de Ellis desactivado.");
        }

        // Enfoque de cámara
        float t = 0f;
        Vector3 initialPos = mainCamera.transform.position;
        Quaternion initialRot = mainCamera.transform.rotation;
        Debug.Log("[FuneralSequence] Moviendo cámara hacia el anillo...");

        while (t < cameraFocusTime)
        {
            t += Time.deltaTime;
            Vector3 direction = (focusPoint.position - mainCamera.transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);

            mainCamera.transform.rotation = Quaternion.Slerp(initialRot, lookRot, t / cameraFocusTime);
            mainCamera.transform.position = Vector3.Lerp(initialPos, focusPoint.position - direction * 1.5f, t / cameraFocusTime);
            yield return null;
        }
        Debug.Log("[FuneralSequence] Cámara enfocada en el anillo.");

        // Fade a negro
        Debug.Log("[FuneralSequence] Iniciando fade a negro.");
        yield return StartCoroutine(FadeMaterialColor(fadeToBlackColor, fadeToBlackTime));
        Debug.Log("[FuneralSequence] Fade a negro completado.");

        // Esperar audio
        if (narrationSource != null)
        {
            Debug.Log("[FuneralSequence] Esperando a que termine el audio...");
            while (narrationSource.isPlaying) yield return null;
            Debug.Log("[FuneralSequence] Audio finalizado.");
        }

        // Fade a blanco y cambio de escena
        Debug.Log("[FuneralSequence] Iniciando fade a blanco.");
        yield return StartCoroutine(FadeMaterialColor(fadeToWhiteColor, fadeToWhiteTime));
        Debug.Log("[FuneralSequence] Fade a blanco completado. Cargando: " + nextSceneName);

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeMaterialColor(Color targetColor, float duration)
    {
        if (fadeMaterial == null)
        {
            Debug.LogWarning("[FuneralSequence] No hay material de fade asignado.");
            yield break;
        }

        Color startColor = fadeMaterial.color;
        Color startEmission = fadeMaterial.GetColor("_EmissionColor");
        Color targetEmission = targetColor * 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            fadeMaterial.color = Color.Lerp(startColor, targetColor, k);
            fadeMaterial.SetColor("_EmissionColor", Color.Lerp(startEmission, targetEmission, k));
            yield return null;
        }

        Debug.Log("[FuneralSequence] Fade completado hacia color: " + targetColor);
    }
}
