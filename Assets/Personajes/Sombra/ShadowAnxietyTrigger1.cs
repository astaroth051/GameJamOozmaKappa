using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ShadowAnxietyTrigger1 : MonoBehaviour
{
    [Header("Configuración de Ansiedad")]
    [Range(0f, 1f)] public float intensityFactor = 0.4f; // más agresivo
    [Range(1f, 5f)] public float fadeOutDelay = 1.2f;

    [Header("Persecución")]
    public float reappearDelayMin = 3f;  // tiempo mínimo para reaparecer
    public float reappearDelayMax = 6f;  // tiempo máximo para reaparecer
    public GameObject shadowObject;      // la sombra a controlar (arrástrala en el inspector)

    [Header("Post-Procesado (Global Volume)")]
    public Volume globalVolume; 
    private SplitToning splitToning;
    private bool isActive = false;
    private bool isChasing = false;

    private void Start()
    {
        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();

        if (globalVolume != null)
            globalVolume.profile.TryGet(out splitToning);

        if (shadowObject != null)
            shadowObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isActive)
            StartCoroutine(TriggerAnxietyEffect(other.transform));
    }

    private IEnumerator TriggerAnxietyEffect(Transform player)
    {
        isActive = true;

        // Activa el post-procesado
        if (splitToning != null)
            splitToning.active = true;

        // Aumenta ansiedad
        var anxiety = FindObjectOfType<AnxietySystem>();
        if (anxiety != null)
        {
            float current = GetPrivateField(anxiety, "anxietyLevel");
            float max = GetPrivateField(anxiety, "maxAnxiety");
            float falta = Mathf.Max(0f, max - current);
            float incremento = falta * intensityFactor;
            float nuevoNivel = Mathf.Clamp(current + incremento, 0f, max);
            SetPrivateField(anxiety, "anxietyLevel", nuevoNivel);
        }

        // Activa la persecución
        if (!isChasing && shadowObject != null)
        {
            shadowObject.SetActive(true);
            StartCoroutine(ShadowChase(player));
        }

        yield return new WaitForSeconds(fadeOutDelay);

        if (splitToning != null)
            splitToning.active = false;

        isActive = false;
    }

    private IEnumerator ShadowChase(Transform player)
    {
        isChasing = true;

        while (shadowObject != null && shadowObject.activeSelf)
        {
            // Persigue constantemente
            Vector3 dir = (player.position - shadowObject.transform.position).normalized;
            shadowObject.transform.position += dir * Time.deltaTime * 3.5f; // velocidad de persecución

            // Si toca al jugador
            if (Vector3.Distance(player.position, shadowObject.transform.position) < 1.2f)
            {
                Debug.Log("[Shadow] Toca al jugador: desaparece y se reactivará luego.");

                shadowObject.SetActive(false);
                yield return new WaitForSeconds(Random.Range(reappearDelayMin, reappearDelayMax));

                // Reaparece en una posición aleatoria cerca del jugador
                Vector3 offset = Random.insideUnitSphere * 6f;
                offset.y = 0f;
                shadowObject.transform.position = player.position + offset;
                shadowObject.SetActive(true);
            }

            yield return null;
        }

        isChasing = false;
    }

    // --- Utilidades para acceder a campos privados ---
    private float GetPrivateField(AnxietySystem target, string fieldName)
    {
        var f = typeof(AnxietySystem).GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return f != null ? (float)f.GetValue(target) : 0f;
    }

    private void SetPrivateField(AnxietySystem target, string fieldName, float value)
    {
        var f = typeof(AnxietySystem).GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(target, value);
    }
}
