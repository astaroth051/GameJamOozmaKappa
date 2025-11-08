using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // nuevo sistema de entrada

public class PauseController : MonoBehaviour
{
    private static PauseController instance;
    private float dpadCooldown = 0.35f;
    private float nextInputTime = 0f;

    private readonly string[] escenasJugables = {
        "PrimerNivel", "SegundoNivel", "TercerNivel", "CuartoNivel", "QuintoNivel"
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        if (!EsEscenaJugable(escenaActual)) return;

        // Detecta tecla ESC en teclado
        bool tecladoPausa = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        // Detecta botón Start / Options en mandos
        bool botonStart = Gamepad.current != null &&
                          (Gamepad.current.startButton.wasPressedThisFrame ||
                           Gamepad.current.selectButton.wasPressedThisFrame);

        // Detecta DPad arriba en cualquier mando
        bool dpadArriba = Gamepad.current != null &&
                          Gamepad.current.dpad.up.wasPressedThisFrame;

        // Evita spam si el jugador mantiene presionado
        bool puedeUsar = Time.time > nextInputTime;
        if (!puedeUsar) return;

        if (tecladoPausa || botonStart || dpadArriba)
        {
            nextInputTime = Time.time + dpadCooldown;
            AbrirPausa();
        }
    }

    private bool EsEscenaJugable(string escena)
    {
        foreach (var e in escenasJugables)
        {
            if (e == escena) return true;
        }
        return false;
    }

    private void AbrirPausa()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", escenaActual);
        PlayerPrefs.Save();

        Time.timeScale = 0f;
        SceneManager.LoadScene("Pausa", LoadSceneMode.Single);
    }
}
