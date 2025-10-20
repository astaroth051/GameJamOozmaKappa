using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class AnxietySystem : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform eyes;                 // Asigna aquí tu Empty "EyePoint"
    [SerializeField] private float viewDistance = 8f;
    [SerializeField] private float viewAngle = 45f;
    [SerializeField] private string anxietyTag = "Ansiedad";
    [SerializeField] private LayerMask detectionMask;

    [Header("Valores de ansiedad")]
    [Range(0, 100)][SerializeField] private float anxietyLevel = 0f;
    [SerializeField] private float anxietyIncreaseRate = 10f;
    [SerializeField] private float anxietyDecreaseRate = 5f;
    [SerializeField] private float maxAnxiety = 100f;
    private bool isOverwhelmed = false;

    [Header("Píldoras")]
    [SerializeField] private float pillReduction = 30f;
    [SerializeField] private int maxPillsBeforeOverdose = 3;
    [SerializeField] private float pillWindowSeconds = 15f;
    private int pillsTaken = 0;
    private float pillTimer = 0f;

    [Header("Efectos visuales (URP Volume)")]
    [SerializeField] private Volume volume;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;
    private Bloom bloom;

    [Header("Efectos auditivos")]
    [SerializeField] private AudioSource heartbeatAudio;
    [SerializeField] private AudioSource breathingAudio;

    [Header("Interfaz")]
    [SerializeField] private Scrollbar anxietyBar;

    [Header("Distorsión temporal")]
    [SerializeField] private float slowMotionScale = 0.6f;
    [SerializeField] private float slowMotionDuration = 2f;

    private void Start()
    {
        if (eyes == null)
        {
            Debug.LogWarning("⚠️ No se asignó un punto de vista (EyePoint). Usa un Empty en la cabeza.");
            eyes = transform;
        }

        if (volume != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdjust);
            volume.profile.TryGet(out bloom);
        }

        ResetVisuals();

        if (heartbeatAudio != null) heartbeatAudio.volume = 0f;
        if (breathingAudio != null) breathingAudio.volume = 0f;

        Debug.Log("✅ AnxietySystem inicializado correctamente. Capa visible: " + LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(detectionMask.value, 2))));
    }

    private void Update()
    {
        HandleRaycast();
        UpdateAnxiety();
        UpdateVisuals();
        UpdateAudio();
        UpdateUI();
    }

    // --- Detecta objetos "Ansiedad" frente al jugador ---
    private void HandleRaycast()
    {
        if (eyes == null) return;

        Ray ray = new Ray(eyes.position, eyes.forward);
        bool hitAnxiety = false;

        // Dispara el raycast solo en la capa especificada
        if (Physics.Raycast(ray, out RaycastHit hit, viewDistance, detectionMask))
        {
            Vector3 dirToTarget = (hit.point - eyes.position).normalized;
            float angle = Vector3.Angle(eyes.forward, dirToTarget);

            // Detecta solo si está dentro del campo visual y tiene el tag correcto
            if (angle < viewAngle && hit.collider.CompareTag(anxietyTag))
            {
                hitAnxiety = true;

                int hitLayer = hit.collider.gameObject.layer;
                string layerName = LayerMask.LayerToName(hitLayer);

                Debug.DrawRay(eyes.position, eyes.forward * viewDistance, Color.red);
                Debug.Log($"👁️ Raycast detectó '{hit.collider.name}' en la capa '{layerName}'.");
            }
            else
            {
                Debug.DrawRay(eyes.position, eyes.forward * viewDistance, Color.green);
            }
        }
        else
        {
            Debug.DrawRay(eyes.position, eyes.forward * viewDistance, Color.green);
        }

        // Ajuste de nivel de ansiedad
        if (hitAnxiety)
            anxietyLevel += anxietyIncreaseRate * Time.deltaTime;
        else
            anxietyLevel -= anxietyDecreaseRate * Time.deltaTime;

        anxietyLevel = Mathf.Clamp(anxietyLevel, 0, maxAnxiety);
    }

    // --- Control de ansiedad y sobredosis ---
    private void UpdateAnxiety()
    {
        pillTimer -= Time.deltaTime;
        if (pillTimer <= 0)
        {
            pillsTaken = 0;
            pillTimer = 0;
        }

        if (anxietyLevel >= maxAnxiety && !isOverwhelmed)
        {
            isOverwhelmed = true;
            StartCoroutine(RestartScene());
        }

        if (anxietyLevel >= 80f && Time.timeScale == 1f)
            StartCoroutine(SlowMotion());
    }

    // --- Llamado desde EllisTankController al tomar píldora ---
    public void TakePill()
    {
        anxietyLevel -= pillReduction;
        anxietyLevel = Mathf.Clamp(anxietyLevel, 0, maxAnxiety);
        pillsTaken++;
        pillTimer = pillWindowSeconds;

        if (pillsTaken >= maxPillsBeforeOverdose)
            StartCoroutine(OverdoseEffect());
    }

    // --- Efectos visuales ---
    private void UpdateVisuals()
    {
        float t = anxietyLevel / maxAnxiety;

        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.value = Mathf.Lerp(0f, 0.5f, t);
        }

        // Desactivado por defecto, se puede usar más adelante
        if (colorAdjust != null && anxietyLevel > 30f) // Solo activa después del 30% de ansiedad
        {
            colorAdjust.active = true;
            colorAdjust.saturation.value = Mathf.Lerp(0f, -60f, t);
        }
        else if (colorAdjust != null && anxietyLevel <= 30f)
        {
            colorAdjust.active = false;
        }
    }

    // --- Efectos auditivos ---
    private void UpdateAudio()
    {
        float t = anxietyLevel / maxAnxiety;

        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = Mathf.Lerp(0f, 1f, t);
            heartbeatAudio.pitch = Mathf.Lerp(1f, 1.4f, t);
        }

        if (breathingAudio != null)
        {
            breathingAudio.volume = Mathf.Lerp(0f, 0.8f, t);
            breathingAudio.pitch = Mathf.Lerp(1f, 1.2f, t);
        }
    }

    // --- Actualiza barra UI ---
    private void UpdateUI()
    {
        if (anxietyBar != null)
            anxietyBar.size = anxietyLevel / maxAnxiety;
    }

    private void ResetVisuals()
    {
        if (vignette != null) vignette.intensity.value = 0f;
        if (colorAdjust != null)
        {
            colorAdjust.saturation.value = 0f;
            colorAdjust.active = false; // evita activación al inicio
        }
        if (bloom != null) bloom.intensity.value = 0f;
    }

    // --- Efectos adicionales ---
    private IEnumerator SlowMotion()
    {
        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }

    private IEnumerator OverdoseEffect()
    {
        if (bloom != null) bloom.active = true;
        float time = 0f;
        while (time < 2f)
        {
            time += Time.deltaTime;
            if (bloom != null) bloom.intensity.value = Mathf.Lerp(0f, 20f, time / 2f);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(RestartScene());
    }

    private IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
