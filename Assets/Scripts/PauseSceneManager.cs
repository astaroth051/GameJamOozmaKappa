using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSceneManager : MonoBehaviour
{
    public Button continuarButton;
    public Button guardarButton;
    public Button opcionesButton;
    public Button menuButton;
    public Button salirButton;

    void Start()
    {
        if (continuarButton) continuarButton.onClick.AddListener(Continuar);
        if (guardarButton) guardarButton.onClick.AddListener(Guardar);
        if (opcionesButton) opcionesButton.onClick.AddListener(AbrirOpciones);
        if (menuButton) menuButton.onClick.AddListener(VolverMenu);
        if (salirButton) salirButton.onClick.AddListener(SalirJuego);
    }

    public void Continuar()
    {
        Time.timeScale = 1f;

        var pauseCtrl = Object.FindFirstObjectByType<PauseController>();
        if (pauseCtrl != null) pauseCtrl.Reanudar();
    }

    public void Guardar()
    {
        SaveSystem.SaveGame();
    }

    public void AbrirOpciones()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Opciones");
    }

    public void VolverMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}
