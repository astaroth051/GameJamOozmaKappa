using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class FuneralSequenceController : MonoBehaviour
{
    [Header("Referencias principales")]
    public GameObject ellis;
    public AudioClip narrationClip;
    public Camera mainCamera;
    public Renderer fadeRenderer;
    public string nextSceneName = "IntroNivel5";

    [Header("Puntos de cámara (en orden)")]
    public List<Transform> focusPoints;   // varios puntos
    public float timePerPoint = 5f;       // duración por punto
    public float pauseBetweenPoints = 0.5f; // pequeña pausa entre puntos

    [Header("Colliders de activación")]
    public Collider firstTrigger;
    public Collider secondTrigger;

    [Header("Transiciones")]
    public float fadeDuration = 3f;

    private bool firstActivated = false;
    private bool secondActivated = false;
    private bool sequenceRunning = false;
    private MonoBehaviour ellisController;
    private Material fadeMaterial;
    private AudioSource narrationSource;
    private TankCameraFollow tankCam;

    private void Start()
    {
        if (ellis != null)
        {
            ellisController = ellis.GetComponent<MonoBehaviour>();
            Debug.Log("[FuneralSequence] Ellis detectado correctamente.");
        }

        if (narrationClip != null)
        {
            narrationSource = gameObject.AddComponent<AudioSource>();
            narrationSource.clip = narrationClip;
            narrationSource.playOnAwake = false;
            narrationSource.spatialBlend = 0f;
            narrationSource.volume = 1f;
        }

        if (fadeRenderer != null)
        {
            fadeMaterial = fadeRenderer.material;
            Color c = fadeMaterial.color;
            c.a = 0f;
            fadeMaterial.color = c;
        }

        if (mainCamera != null)
            tankCam = mainCamera.GetComponent<TankCameraFollow>();
    }

    public void OnZoneEntered(bool isFirstZone)
    {
        if (isFirstZone && !firstActivated)
        {
            firstActivated = true;
            StartCoroutine(PlayFirstAudio());
        }
        else if (!isFirstZone && !secondActivated)
        {
            secondActivated = true;
            StartCoroutine(FinalSequence());
        }
    }

    private IEnumerator PlayFirstAudio()
    {
        if (narrationSource == null || narrationSource.clip == null) yield break;
        narrationSource.Play();
        Debug.Log("[FuneralSequence] Reproduciendo audio: " + narrationSource.clip.name);
    }

    private IEnumerator FinalSequence()
    {
        if (sequenceRunning) yield break;
        sequenceRunning = true;

        Debug.Log("[FuneralSequence] Iniciando secuencia final.");

        if (ellisController != null)
            ellisController.enabled = false;

        if (tankCam != null)
        {
            tankCam.freezeCamera = true;
            Debug.Log("[FuneralSequence] Cámara congelada.");
        }

        // --- Recorre cada punto suavemente ---
        if (focusPoints != null && focusPoints.Count > 0)
        {
            for (int i = 0; i < focusPoints.Count; i++)
            {
                Transform point = focusPoints[i];
                if (point == null) continue;

                Vector3 startPos = mainCamera.transform.position;
                Quaternion startRot = mainCamera.transform.rotation;
                float elapsed = 0f;

                while (elapsed < timePerPoint)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / timePerPoint);

                    // Suavizado tipo ease-in/out (más cinematográfico)
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    mainCamera.transform.position = Vector3.Lerp(startPos, point.position, smoothT);
                    mainCamera.transform.rotation = Quaternion.Slerp(startRot, point.rotation, smoothT);
                    yield return null;
                }

                mainCamera.transform.position = point.position;
                mainCamera.transform.rotation = point.rotation;

                if (pauseBetweenPoints > 0)
                    yield return new WaitForSeconds(pauseBetweenPoints);
            }
        }

        // Esperar fin del audio antes del fade
        if (narrationSource != null)
        {
            while (narrationSource.isPlaying)
                yield return null;
        }

        Debug.Log("[FuneralSequence] Audio finalizado, iniciando fade...");

        yield return StartCoroutine(FadeInMaterial(fadeDuration));

        Debug.Log("[FuneralSequence] Fade completado. Cargando siguiente escena...");
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeInMaterial(float duration)
    {
        if (fadeMaterial == null) yield break;

        Color c = fadeMaterial.color;
        float startAlpha = c.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(startAlpha, 1f, t);
            fadeMaterial.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeMaterial.color = c;
    }
}
