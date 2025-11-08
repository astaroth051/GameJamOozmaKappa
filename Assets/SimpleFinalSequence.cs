using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleFinalSequence : MonoBehaviour
{
    [Header("Referencias principales")]
    public GameObject ellis;
    public Camera mainCamera;
    public Transform cinematicPoint;   // Punto del cuadro (posición + rotación)
    public Renderer fadeRenderer;      // Plano con material transparente
    public AudioClip narrationClip;
    public string nextSceneName = "Credits";

    [Header("Tiempos")]
    public float moveTime = 4f;        // Tiempo de transición hacia el cuadro
    public float stayTime = 15f;       // Tiempo que la cámara permanece fija
    public float fadeTime = 3f;        // Tiempo del fade a negro

    private bool triggered = false;
    private AudioSource narrationSource;
    private Material fadeMaterial;
    private TankCameraFollow tankCam;

    private void Start()
    {
        // Inicializar AudioSource
        if (narrationClip != null)
        {
            narrationSource = gameObject.AddComponent<AudioSource>();
            narrationSource.clip = narrationClip;
            narrationSource.playOnAwake = false;
            narrationSource.spatialBlend = 0f;
            narrationSource.volume = 1f;
        }

        // Inicializar material del fade
        if (fadeRenderer != null)
        {
            fadeMaterial = fadeRenderer.material;
            Color c = fadeMaterial.color;
            c.a = 0f; // Comienza transparente
            fadeMaterial.color = c;
        }

        // Obtener el controlador de cámara
        if (mainCamera != null)
            tankCam = mainCamera.GetComponent<TankCameraFollow>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(FinalSequence());
    }

    private IEnumerator FinalSequence()
    {
        Debug.Log("[SimpleFinalSequence] Secuencia final iniciada.");

        // Desactivar control del jugador
        if (ellis != null)
        {
            MonoBehaviour ctrl = ellis.GetComponent<MonoBehaviour>();
            if (ctrl != null) ctrl.enabled = false;
        }

        // Congelar cámara del sistema de seguimiento
        if (tankCam != null)
        {
            tankCam.freezeCamera = true;
            Debug.Log("[SimpleFinalSequence] Cámara congelada para cinematic.");
        }

        // Reproducir narración
        if (narrationSource != null)
            narrationSource.Play();

        // Movimiento hacia el cuadro
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, cinematicPoint.position, smooth);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, cinematicPoint.rotation, smooth);
            yield return null;
        }

        // Fijar posición final exacta
        mainCamera.transform.position = cinematicPoint.position;
        mainCamera.transform.rotation = cinematicPoint.rotation;

        Debug.Log("[SimpleFinalSequence] Cámara fijada en el cuadro.");

        // Esperar tiempo fijo (15 s)
        yield return new WaitForSeconds(stayTime);

        // Fade a negro
        if (fadeMaterial != null)
        {
            Color c = fadeMaterial.color;
            float alphaStart = c.a;
            float time = 0f;

            while (time < fadeTime)
            {
                time += Time.deltaTime;
                float k = Mathf.Clamp01(time / fadeTime);
                c.a = Mathf.Lerp(alphaStart, 1f, k);
                fadeMaterial.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeMaterial.color = c;
        }

        Debug.Log("[SimpleFinalSequence] Cargando escena de créditos...");
        SceneManager.LoadScene(nextSceneName);
    }
}
