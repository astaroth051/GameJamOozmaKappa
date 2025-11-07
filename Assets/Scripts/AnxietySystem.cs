using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class AnxietySystem : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform eyes;
    [SerializeField] private float viewDistance = 8f;
    [SerializeField] private float viewAngle = 45f;
    [SerializeField] private string anxietyTag = "Ansiedad";
    [SerializeField] private string shadowTag = "Sombra";
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] private LayerMask shadowMask;

    [Header("Valores de ansiedad")]
    [Range(0, 100)][SerializeField] private float anxietyLevel = 0f;
    [SerializeField] private float anxietyIncreaseRate = 10f;
    [SerializeField] private float shadowAnxietyRate = 35f;
    [SerializeField] private float anxietyDecreaseRate = 2.5f;
    [SerializeField] private float maxAnxiety = 100f;
    private bool isOverwhelmed = false;
    private bool touchingAnxiety = false;
    private bool touchingShadow = false;
    private bool bloquearAnsiedad = false;

    [Header("Píldoras")]
    [SerializeField] private float pillDecaySeconds = 15f;
    [SerializeField] private float pillReduction = 30f;
    [SerializeField] private int maxPillsBeforeOverdose = 3;
    [SerializeField] private float pillDecayRate = 0.05f;
    private float currentPillLevel = 0f;
    private bool isFlashingPill = false;

    [Header("Efectos visuales (URP Volume)")]
    [SerializeField] private Volume volume;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;
    private Bloom bloom;
    private FilmGrain filmGrain;

    [Header("Efectos auditivos")]
    [SerializeField] private AudioSource heartbeatAudio;
    [SerializeField] private AudioSource breathingAudio;

    [Header("Interfaz")]
    [SerializeField] private Scrollbar anxietyBar;
    [SerializeField] private Scrollbar pillBar;

    [Header("Distorsión temporal")]
    [SerializeField] private float slowMotionScale = 0.6f;
    [SerializeField] private float slowMotionDuration = 2f;

    [Header("Fades con planos")]
    [Tooltip("Plano para fade general (negro).")]
    public Renderer fadePlaneRenderer;

    [Tooltip("Plano para efecto de medicación.")]
    public Renderer fadePillRenderer;

    private bool isFading = false;
    private bool isPillFading = false;

    private Material fadeMaterial;
    private Material fadePillMaterial;

    private float fadeAlpha = 0f;
    private float fadePillAlpha = 0f;

    private void Start()
    {
        if (eyes == null)
            eyes = transform;

        // --- Materiales desde el inspector ---
        if (fadePlaneRenderer != null)
            fadeMaterial = fadePlaneRenderer.material;
        if (fadePillRenderer != null)
            fadePillMaterial = fadePillRenderer.material;

        // --- Postproceso ---
        if (volume != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdjust);
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out filmGrain);
        }

        ResetVisuals();

        // --- Audio base ---
        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = 0f;
            heartbeatAudio.loop = true;
            heartbeatAudio.Play();
        }

        if (breathingAudio != null)
        {
            breathingAudio.volume = 0f;
            breathingAudio.loop = true;
            breathingAudio.Play();
        }

        if (pillBar != null)
            pillBar.size = 0f;

        // --- Fades iniciales transparentes ---
        if (fadeMaterial != null)
            fadeMaterial.color = new Color(0, 0, 0, 0);

        if (fadePillMaterial != null)
            fadePillMaterial.color = new Color(0, 0, 0, 0);
    }

    private void Update()
    {
        HandleRaycast();
        HandleTriggerAnxiety();
        HandleTriggerShadow();
        UpdateAnxiety();
        UpdateVisuals();
        UpdateAudio();
        UpdatePillDecay();
        UpdateUI();

        // Actualiza color de los planos
        if (fadeMaterial != null)
            fadeMaterial.color = new Color(0, 0, 0, fadeAlpha);

        if (fadePillMaterial != null)
            fadePillMaterial.color = new Color(0, 0, 0, fadePillAlpha);
    }

    // --- Sistema de raycast ---
    private void HandleRaycast()
    {
        if (eyes == null || bloquearAnsiedad) return;

        Ray ray = new Ray(eyes.position, eyes.forward);
        bool hitAnxiety = false;
        bool hitShadow = false;

        if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
        {
            Vector3 dirToTarget = (hit.point - eyes.position).normalized;
            float angle = Vector3.Angle(eyes.forward, dirToTarget);

            bool esAnsiedad = hit.collider.CompareTag(anxietyTag) ||
                              ((detectionMask.value & (1 << hit.collider.gameObject.layer)) != 0);

            bool esSombra = hit.collider.CompareTag(shadowTag) ||
                            ((shadowMask.value & (1 << hit.collider.gameObject.layer)) != 0);

            if (angle < viewAngle)
            {
                if (esAnsiedad) hitAnxiety = true;
                if (esSombra) hitShadow = true;
            }
        }

        if (hitAnxiety && !hitShadow)
            anxietyLevel += anxietyIncreaseRate * Time.deltaTime;
        else if (hitShadow)
            anxietyLevel += shadowAnxietyRate * Time.deltaTime;
        else if (!touchingAnxiety && !touchingShadow)
            anxietyLevel -= anxietyDecreaseRate * Time.deltaTime;

        anxietyLevel = Mathf.Clamp(anxietyLevel, 0, maxAnxiety);
    }

    private void HandleTriggerAnxiety()
    {
        if (bloquearAnsiedad) return;
        if (touchingAnxiety)
            anxietyLevel += anxietyIncreaseRate * 1.2f * Time.deltaTime;
        anxietyLevel = Mathf.Clamp(anxietyLevel, 0, maxAnxiety);
    }

    private void HandleTriggerShadow()
    {
        if (bloquearAnsiedad) return;
        if (touchingShadow)
            anxietyLevel += shadowAnxietyRate * 1.2f * Time.deltaTime;
        anxietyLevel = Mathf.Clamp(anxietyLevel, 0, maxAnxiety);
    }

    // --- Colisiones ---
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<Collider>(out Collider col))
        {
            if (col.gameObject.CompareTag(shadowTag))
                touchingShadow = true;
            else if (col.gameObject.CompareTag(anxietyTag))
                touchingAnxiety = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<Collider>(out Collider col))
        {
            if (col.gameObject.CompareTag(shadowTag))
                touchingShadow = false;
            else if (col.gameObject.CompareTag(anxietyTag))
                touchingAnxiety = false;
        }
    }

    // --- Ansiedad y saturación ---
    private void UpdateAnxiety()
    {
        if (anxietyLevel >= maxAnxiety && !isOverwhelmed)
        {
            isOverwhelmed = true;
            StartCoroutine(SlowMotion());
            var ellis = FindObjectOfType<EllisTankController>();
            if (ellis != null)
                ellis.TriggerAnxietyFocus();
        }
    }

    // --- Tomar píldora ---
    public void TakePill()
    {
        StartCoroutine(DelayedPillEffect());
        StartCoroutine(FadePillEffect());

        var sombra = FindObjectOfType<ShadowController>();
        if (sombra != null)
            sombra.StartCoroutine(sombra.FadeOutAndReappear());
    }

    private IEnumerator DelayedPillEffect()
    {
        bloquearAnsiedad = true;
        float originalDecrease = anxietyDecreaseRate;
        float originalDecay = pillDecayRate;

        anxietyDecreaseRate *= 0.25f;
        pillDecayRate *= 0.25f;

        yield return new WaitForSeconds(5f);

        anxietyLevel -= pillReduction;
        anxietyLevel = Mathf.Clamp(anxietyLevel, 0, maxAnxiety);

        currentPillLevel += 1f / maxPillsBeforeOverdose;
        currentPillLevel = Mathf.Clamp01(currentPillLevel);

        if (pillBar != null) pillBar.size = currentPillLevel;

        if (colorAdjust != null && !isFlashingPill)
            StartCoroutine(PillVisualFlash());

        if (currentPillLevel >= 1f)
            StartCoroutine(OverdoseEffect());

        yield return new WaitForSeconds(pillDecaySeconds * 0.5f);

        anxietyDecreaseRate = originalDecrease;
        pillDecayRate = originalDecay;
        bloquearAnsiedad = false;
    }

    // --- Efecto visual de la píldora ---
    private IEnumerator FadePillEffect()
    {
        if (isPillFading || fadePillMaterial == null) yield break;
        isPillFading = true;

        float fadeInDuration = 1.2f;
        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            fadePillAlpha = Mathf.Lerp(0f, 0.8f, time / fadeInDuration);
            yield return null;
        }

        yield return new WaitForSeconds(1.8f);

        float fadeOutDuration = 1.2f;
        time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            fadePillAlpha = Mathf.Lerp(0.8f, 0f, time / fadeOutDuration);
            yield return null;
        }

        fadePillAlpha = 0f;
        isPillFading = false;
    }

    // --- Efecto visual de sobredosis ---
    private IEnumerator OverdoseEffect()
    {
        if (bloom != null) bloom.active = true;
        float time = 0f;
        while (time < 2f)
        {
            time += Time.deltaTime;
            if (bloom != null) bloom.intensity.value = Mathf.Lerp(0f, 25f, time / 2f);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(RestartScene());
    }

    private IEnumerator RestartScene()
    {
        yield return StartCoroutine(FadeToBlack(2f));
        yield return new WaitForSeconds(0.5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("PrimerNivel");
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (isFading || fadeMaterial == null) yield break;
        isFading = true;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            fadeAlpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        fadeAlpha = 1f;
        isFading = false;
    }

    // --- Otros efectos visuales ---
    private void UpdatePillDecay()
    {
        if (currentPillLevel > 0)
        {
            currentPillLevel -= pillDecayRate * Time.deltaTime / pillDecaySeconds;
            currentPillLevel = Mathf.Max(0f, currentPillLevel);
            if (pillBar != null) pillBar.size = currentPillLevel;
        }
    }

    private IEnumerator PillVisualFlash()
    {
        isFlashingPill = true;
        float time = 0f;
        float duration = 1.8f;
        float startSat = colorAdjust.saturation.value;

        while (time < duration / 2f)
        {
            time += Time.deltaTime;
            float t = time / (duration / 2f);
            colorAdjust.saturation.value = Mathf.Lerp(startSat, -80f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            colorAdjust.saturation.value = Mathf.Lerp(-80f, 0f, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        isFlashingPill = false;
    }

    private void UpdateVisuals()
    {
        float t = anxietyLevel / maxAnxiety;
        float easedT = Mathf.SmoothStep(0f, 1f, t);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0f, 0.8f, easedT);

        if (colorAdjust != null && !isFlashingPill)
        {
            if (anxietyLevel >= 80f)
            {
                float localT = Mathf.InverseLerp(80f, 100f, anxietyLevel);
                colorAdjust.saturation.value = Mathf.Lerp(0f, -100f, Mathf.Pow(localT, 1.5f));
            }
            else
            {
                colorAdjust.saturation.value = Mathf.Lerp(colorAdjust.saturation.value, 0f, Time.deltaTime * 3f);
            }
        }

        if (filmGrain != null)
        {
            if (anxietyLevel > 30f)
            {
                filmGrain.active = true;
                float grainT = Mathf.InverseLerp(30f, 100f, anxietyLevel);
                filmGrain.intensity.value = Mathf.Lerp(0f, 1f, grainT);
            }
            else
            {
                filmGrain.active = false;
                filmGrain.intensity.value = 0f;
            }
        }
    }

    private void UpdateAudio()
    {
        float t = anxietyLevel / maxAnxiety;
        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = t > 0.5f ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0.5f, 1f, t)) : 0f;
            heartbeatAudio.pitch = Mathf.Lerp(1f, 1.4f, t);
        }
        if (breathingAudio != null)
        {
            breathingAudio.volume = t > 0.5f ? Mathf.Lerp(0f, 0.8f, Mathf.InverseLerp(0.5f, 1f, t)) : 0f;
            breathingAudio.pitch = Mathf.Lerp(1f, 1.2f, t);
        }
    }

    private void UpdateUI()
    {
        if (anxietyBar != null)
            anxietyBar.size = anxietyLevel / maxAnxiety;
    }
    private void ResetVisuals()
    {
        if (vignette != null) { vignette.active = true; vignette.intensity.value = 0f; }
        if (colorAdjust != null) { colorAdjust.active = true; colorAdjust.saturation.value = 0f; }
        if (bloom != null) { bloom.active = true; bloom.intensity.value = 0f; }
        if (filmGrain != null) { filmGrain.active = true; filmGrain.intensity.value = 0f; }
    }

    private IEnumerator SlowMotion()
    {
        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }
    // --- Métodos usados por PlayerTriggerRelay ---
    public void NotifyTriggerEnter(Collider other)
    {
        if (other.CompareTag(shadowTag))
            touchingShadow = true;
        else if (other.CompareTag(anxietyTag))
            touchingAnxiety = true;
    }

    public void NotifyTriggerStay(Collider other)
    {
        if (other.CompareTag(shadowTag))
            touchingShadow = true;
        else if (other.CompareTag(anxietyTag))
            touchingAnxiety = true;
    }

    public void NotifyTriggerExit(Collider other)
    {
        if (other.CompareTag(shadowTag))
            touchingShadow = false;
        else if (other.CompareTag(anxietyTag))
            touchingAnxiety = false;
    }

}