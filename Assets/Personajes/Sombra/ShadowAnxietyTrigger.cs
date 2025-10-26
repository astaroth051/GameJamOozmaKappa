using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ShadowAnxietyTrigger : MonoBehaviour
{
    [Header("Configuración de Ansiedad")]
    [Range(0f, 1f)] public float intensityFactor = 0.3f;


    [Range(0.5f, 3f)] public float fadeOutDelay = 1f;

    [Header("Post-Procesado (Global Volume)")]
    public Volume globalVolume; // arrastra tu Volume con Split Toning
    private SplitToning splitToning;
    private bool isActive = false;

    private void Start()
    {
        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();

        if (globalVolume != null)
            globalVolume.profile.TryGet(out splitToning);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(TriggerAnxietyEffect());
    }

    private IEnumerator TriggerAnxietyEffect()
    {
        isActive = true;

        //  Activa Split Toning al instante
        if (splitToning != null)
            splitToning.active = true;

        //  Aumenta la ansiedad proporcionalmente
        var anxiety = FindObjectOfType<AnxietySystem>();
        if (anxiety != null)
        {
            float current = GetPrivateField(anxiety, "anxietyLevel");
            float max = GetPrivateField(anxiety, "maxAnxiety");
            float falta = Mathf.Max(0f, max - current);

            // incremento proporcional a lo que falta
            float incremento = falta * intensityFactor;
            float nuevoNivel = Mathf.Clamp(current + incremento, 0f, max);

            SetPrivateField(anxiety, "anxietyLevel", nuevoNivel);
            Debug.Log($" Sombra tocada — ansiedad subió de {current:F1} a {nuevoNivel:F1}");
        }

        //  Espera un segundo, luego apaga Split Toning
        yield return new WaitForSeconds(fadeOutDelay);

        if (splitToning != null)
            splitToning.active = false;

        isActive = false;
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
