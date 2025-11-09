using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Paneles del Canvas")]
    public GameObject panelPausa;
    public GameObject panelJuego;

    [Header("Configuración")]
    public float cooldown = 0.25f;

    private bool enPausa = false;
    private float nextTime = 0f;

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

        if (!EsEscenaJugable(escena))
        {
            this.enabled = false;
            return;
        }

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);
    }

    private bool EsEscenaJugable(string escenaActual)
    {
        foreach (string s in escenasJugables)
            if (escenaActual == s) return true;
        return false;
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;

        bool pressed = false;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            pressed = true;

        if (Gamepad.current != null)
        {
            var gp = Gamepad.current;
            if (gp.startButton.wasPressedThisFrame) pressed = true;
            if (gp.selectButton.wasPressedThisFrame) pressed = true;
            if (gp.dpad.up.wasPressedThisFrame) pressed = true;
        }

        if (pressed)
        {
            nextTime = Time.unscaledTime + cooldown;
            if (enPausa) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        if (enPausa) return;
        enPausa = true;

        Time.timeScale = 0f;

        EllisTankController ellis = FindFirstObjectByType<EllisTankController>();
        if (ellis != null) ellis.enabled = false;

        if (panelPausa != null) panelPausa.SetActive(true);
        if (panelJuego != null) panelJuego.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Reanudar()
    {
        enPausa = false;
        Time.timeScale = 1f;

        EllisTankController ellis = FindFirstObjectByType<EllisTankController>();
        if (ellis != null) ellis.enabled = true;

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);
    }

    public void SalirMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}
