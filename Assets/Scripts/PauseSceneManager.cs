using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSceneManager : MonoBehaviour
{
    [Header("Botones del menú de pausa")]
    public Button continuarButton;
    public Button guardarButton;
    public Button opcionesButton;
    public Button menuButton;
    public Button salirButton;

    private string lastScene;

    void Start()
    {
        // Recupera la escena desde la que se pausó
        lastScene = PlayerPrefs.GetString("LastScene", "PrimerNivel");

        // Asignar acciones a cada botón
        if (continuarButton) continuarButton.onClick.AddListener(Continuar);
        if (guardarButton) guardarButton.onClick.AddListener(Guardar);
        if (opcionesButton) opcionesButton.onClick.AddListener(AbrirOpciones);
        if (menuButton) menuButton.onClick.AddListener(VolverMenu);
        if (salirButton) salirButton.onClick.AddListener(SalirJuego);
    }

    public void Continuar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lastScene);
    }

    public void Guardar()
    {
        var anxietySystem = FindObjectOfType<AnxietySystem>();
        float ansiedadActual = 0f;

        if (anxietySystem != null)
        {
            var field = typeof(AnxietySystem).GetField("anxietyLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                ansiedadActual = (float)field.GetValue(anxietySystem);
        }

        SaveSystem.SaveGame(ansiedadActual, lastScene);
        Debug.Log($"Partida guardada desde la escena de pausa ({lastScene})");
    }

    public void AbrirOpciones()
    {
        // Guarda la escena de pausa para volver
        PlayerPrefs.SetString("LastScene", "Pausa");
        PlayerPrefs.Save();

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
