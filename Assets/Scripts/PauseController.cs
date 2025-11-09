using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Paneles del Canvas")]
    public GameObject panelPausa;   // Panel del menú de pausa
    public GameObject panelJuego;   // HUD o UI principal del juego

    [Header("Configuración")]
    public float cooldown = 0.25f;

    private bool enPausa = false;
    private float nextTime = 0f;

    // -----------------------------
    // ESCENAS DONDE SÍ FUNCIONA PAUSA
    // -----------------------------
    private readonly string[] escenasJugables =
    {
        "PrimerNivel",
        "SegundoNivel",
        "TercerNivel",
        "CuartoNivel",
        "QuintoNivel"
    };

    void Start()
    {
        string escena = SceneManager.GetActiveScene().name;

        // Si NO es escena jugable → Desactivar PauseController
        if (!EsEscenaJugable(escena))
        {
            Debug.Log("[PauseController] Desactivado en escena: " + escena);
            this.enabled = false;
            return;
        }

        Debug.Log("[PauseController] Activo en escena jugable: " + escena);

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);
    }

    private bool EsEscenaJugable(string escenaActual)
    {
        foreach (string s in escenasJugables)
        {
            if (escenaActual == s)
                return true;
        }
        return false;
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;

        bool pressed = false;

        // ---------------------------
        // TECLADO
        // ---------------------------
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            pressed = true;

        // ---------------------------
        // GAMEPAD (PS2/3/4/5 - Xbox - Genéricos)
        // ---------------------------
        if (Gamepad.current != null)
        {
            var gp = Gamepad.current;

            // Start / Options
            if (gp.startButton.wasPressedThisFrame) pressed = true;

            // Select / Share / Back
            if (gp.selectButton.wasPressedThisFrame) pressed = true;

            // D-Pad UP (único botón direccional activador)
            if (gp.dpad.up.wasPressedThisFrame) pressed = true;
        }

        // ---------------------------
        // EJECUTAR PAUSA / REANUDAR
        // ---------------------------
        if (pressed)
        {
            nextTime = Time.unscaledTime + cooldown;
            if (enPausa) Reanudar();
            else Pausar();
        }
    }



    // -------------------------------------------------------------
    // MÉTODOS DE PAUSA Y REANUDACIÓN
    // -------------------------------------------------------------
    public void Pausar()
    {
        if (enPausa) return;
        enPausa = true;

        Time.timeScale = 0f;
        MusicManager.SilenciarTodo();

        EllisTankController ellis = FindFirstObjectByType<EllisTankController>();
        if (ellis != null)
        {
            ellis.PausarTodosLosSonidos();
            ellis.enabled = false;
        }

        if (panelPausa != null) panelPausa.SetActive(true);
        if (panelJuego != null) panelJuego.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Reanudar()
    {
        enPausa = false;
        Time.timeScale = 1f;
        MusicManager.RestaurarVolumen();

        EllisTankController ellis = FindFirstObjectByType<EllisTankController>();
        if (ellis != null)
        {
            ellis.ReanudarTodosLosSonidos();
            ellis.enabled = true;
        }

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);
    }

    // -------------------------------------------------------------
    // SALIR Y MENÚ
    // -------------------------------------------------------------
    public void SalirMenu()
    {
        Time.timeScale = 1f;
        MusicManager.RestaurarVolumen();
        SceneManager.LoadScene("Menu");
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}
