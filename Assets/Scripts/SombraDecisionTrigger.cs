using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class SombraDecisionTrigger : MonoBehaviour
{
    [Header("Referencias UI")]
    public Canvas canvasOpciones;
    public TextMeshProUGUI textoOpcionA;
    public TextMeshProUGUI textoOpcionB;
    public TextMeshProUGUI textoSombra;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clipSombraInicio;   // "Podemos cambiarlo..."
    public AudioClip clipAceptar;        // "Ahora sí quiero"
    public AudioClip clipRechazar;       // sonido de rechazo

    [Header("Fade")]
    public Renderer fadeRenderer;
    public float fadeDuration = 2f;

    [Header("Escenas destino")]
    public string escenaVolver = "IntroNivel1";
    public string escenaSeguir = "IntroNivel4";

    [Header("Tiempos")]
    public float tiempoTextoExtra = 1.5f;

    private bool usandoGamepad = false;
    private bool dentro = false;
    private bool decisionActiva = false;
    private AudioSource[] otrosAudios;

    void Start()
    {
        if (canvasOpciones != null)
            canvasOpciones.enabled = false;

        if (textoOpcionA != null)
            textoOpcionA.gameObject.SetActive(false);

        if (textoOpcionB != null)
            textoOpcionB.gameObject.SetActive(false);

        if (textoSombra != null)
        {
            textoSombra.text = "";
            textoSombra.gameObject.SetActive(true);
            Color c = textoSombra.color;
            c.a = 0f;
            textoSombra.color = c;
        }

        if (fadeRenderer != null)
        {
            Color c = fadeRenderer.material.color;
            c.a = 0f;
            fadeRenderer.material.color = c;
        }
    }

    void Update()
    {
        DetectarEntrada();

        if (!decisionActiva) return;

        if (!usandoGamepad)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                    StartCoroutine(Rechazar());
                else if (Keyboard.current.rKey.wasPressedThisFrame)
                    StartCoroutine(Aceptar());
            }
        }
        else
        {
            var gp = Gamepad.current;
            if (gp == null) return;

            if (gp.squareButton.wasPressedThisFrame || gp.buttonWest.wasPressedThisFrame)
                StartCoroutine(Rechazar());
            else if (gp.circleButton.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame)
                StartCoroutine(Aceptar());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (dentro) return;
        if (!other.CompareTag("Player")) return;
        dentro = true;

        // 🔹 Apenas colisiona → reproduce voz y texto inmediatamente
        StartCoroutine(ReproducirInicio());
        // 🔹 Luego de unos segundos se muestran las opciones
        StartCoroutine(MostrarOpciones());
    }

    IEnumerator MostrarOpciones()
    {
        yield return new WaitForSeconds(Mathf.Max(clipSombraInicio != null ? clipSombraInicio.length : 2f, 2f) + 0.5f);

        if (canvasOpciones != null)
            canvasOpciones.enabled = true;

        if (textoOpcionA != null)
            textoOpcionA.gameObject.SetActive(true);

        if (textoOpcionB != null)
            textoOpcionB.gameObject.SetActive(true);

        ActualizarTexto();
        decisionActiva = true;
    }

    void DetectarEntrada()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
            usandoGamepad = false;
        else if (Gamepad.current != null && Gamepad.current.allControls.Any(c => c.IsPressed()))
            usandoGamepad = true;
    }

    void ActualizarTexto()
    {
        if (textoOpcionA == null || textoOpcionB == null) return;

        if (usandoGamepad)
        {
            textoOpcionA.text = "Cuadrado (gamepad) / E (teclado) = Rechazar";
            textoOpcionB.text = "Redondo (gamepad) / R (teclado) = Volver";
        }
        else
        {
            textoOpcionA.text = "E (teclado) / Cuadrado (gamepad) = Rechazar";
            textoOpcionB.text = "R (teclado) / Redondo (gamepad) = Volver";
        }
    }

    IEnumerator Rechazar()
    {
        decisionActiva = false;
        yield return StartCoroutine(ReproducirVoz(clipRechazar, null));
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(escenaSeguir);
    }

    IEnumerator Aceptar()
    {
        decisionActiva = false;
        yield return StartCoroutine(ReproducirVoz(clipAceptar, null));
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(escenaVolver);
    }

    IEnumerator FadeOut()
    {
        if (fadeRenderer == null) yield break;
        float t = 0f;
        Color c = fadeRenderer.material.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeRenderer.material.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeRenderer.material.color = c;
    }

    // 🔹 Inicio (voz + texto sincronizados)
    IEnumerator ReproducirInicio()
    {
        if (audioSource == null || clipSombraInicio == null) yield break;

        otrosAudios = FindObjectsOfType<AudioSource>();
        foreach (var a in otrosAudios)
        {
            if (a != null && a != audioSource)
                a.mute = true;
        }

        // Empieza el audio y el texto a la vez
        audioSource.volume = 6f;
        audioSource.clip = clipSombraInicio;
        audioSource.Play();

        // Aparece el texto de inmediato
        if (textoSombra != null)
        {
            textoSombra.text = "Podemos cambiarlo... solo vuelve atrás, cierra los ojos y déjame conducir.";
            textoSombra.gameObject.SetActive(true);

            // Fade in visual
            Color c = textoSombra.color;
            for (float i = 0; i < 1f; i += Time.deltaTime)
            {
                c.a = Mathf.Lerp(0f, 1f, i);
                textoSombra.color = c;
                yield return null;
            }
        }

        // Mantiene el texto visible mientras dura el audio
        yield return new WaitForSeconds(clipSombraInicio.length + tiempoTextoExtra);

        // Fade out del texto
        if (textoSombra != null)
        {
            Color c = textoSombra.color;
            for (float i = 0; i < 1f; i += Time.deltaTime)
            {
                c.a = Mathf.Lerp(1f, 0f, i);
                textoSombra.color = c;
                yield return null;
            }
            textoSombra.text = "";
        }

        foreach (var a in otrosAudios)
        {
            if (a != null && a != audioSource)
                a.mute = false;
        }
    }

    // 🔹 Reproduce voz genérica (para aceptar o rechazar)
    IEnumerator ReproducirVoz(AudioClip clip, string texto)
    {
        if (audioSource == null || clip == null) yield break;

        otrosAudios = FindObjectsOfType<AudioSource>();
        foreach (var a in otrosAudios)
        {
            if (a != null && a != audioSource)
                a.mute = true;
        }

        audioSource.volume = 6f;
        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitForSeconds(Mathf.Max(clip.length, 3f) + tiempoTextoExtra);

        foreach (var a in otrosAudios)
        {
            if (a != null && a != audioSource)
                a.mute = false;
        }
    }
}
