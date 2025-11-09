using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSceneManager : MonoBehaviour
{
    [Header("Botones del menú de pausa")]
    public Button continuarButton;
    public Button guardarButton;     // ★ NUEVO
    public Button opcionesButton;
    public Button menuButton;
    public Button salirButton;

    private AudioSource[] audiosEnEscena;

    void Start()
    {
        if (continuarButton) continuarButton.onClick.AddListener(Continuar);
        if (guardarButton) guardarButton.onClick.AddListener(Guardar);   // ★ NUEVO
        if (opcionesButton) opcionesButton.onClick.AddListener(AbrirOpciones);
        if (menuButton) menuButton.onClick.AddListener(VolverMenu);
        if (salirButton) salirButton.onClick.AddListener(SalirJuego);

        PausarAudios();
    }

    // ----------------------------------------------------
    // PAUSAR / REANUDAR AUDIO
    // ----------------------------------------------------
    private void PausarAudios()
    {
        audiosEnEscena = FindObjectsOfType<AudioSource>();

        foreach (AudioSource a in audiosEnEscena)
            if (a.isPlaying) a.Pause();

        Debug.Log("[PauseSceneManager] Todos los audios pausados.");
    }

    private void ReanudarAudios()
    {
        if (audiosEnEscena == null) return;

        foreach (AudioSource a in audiosEnEscena)
            if (a != null) a.UnPause();

        Debug.Log("[PauseSceneManager] Audios reanudados.");
    }

    // ----------------------------------------------------
    // BOTÓN CONTINUAR
    // ----------------------------------------------------
    public void Continuar()
    {
        Time.timeScale = 1f;
        ReanudarAudios();

        var pauseCtrl = Object.FindFirstObjectByType<PauseController>();
        if (pauseCtrl != null) pauseCtrl.Reanudar();

        Debug.Log("[PauseSceneManager] Continuando juego.");
    }

    // ----------------------------------------------------
    // ★ BOTÓN GUARDAR
    // ----------------------------------------------------
    public void Guardar()
    {
        Time.timeScale = 1f; // seguridad
        SaveSystem.SaveGame();

        Debug.Log("[PauseSceneManager] Partida guardada desde el menú de pausa.");
    }

    // ----------------------------------------------------
    // BOTÓN OPCIONES
    // ----------------------------------------------------
    public void AbrirOpciones()
    {
        Time.timeScale = 1f;
        ReanudarAudios();
        SceneManager.LoadScene("Opciones");
    }

    // ----------------------------------------------------
    // VOLVER AL MENÚ
    // ----------------------------------------------------
    public void VolverMenu()
    {
        Time.timeScale = 1f;
        ReanudarAudios();
        SceneManager.LoadScene("Menu");
    }

    // ----------------------------------------------------
    // SALIR DEL JUEGO
    // ----------------------------------------------------
    public void SalirJuego()
    {
        Application.Quit();
    }
}
