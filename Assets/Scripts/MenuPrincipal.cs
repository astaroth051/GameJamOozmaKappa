using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuPrincipal : MonoBehaviour
{
    [System.Serializable]
    public class MenuButton
    {
        public string buttonName;   // Nombre descriptivo
        public Button button;       // Referencia al botón en el Canvas
        public ButtonAction action; // Acción que ejecutará
        public string targetScene;  // Nombre de la escena (si aplica)
    }

    public enum ButtonAction
    {
        None,
        LoadScene,
        LoadSavedGame,
        NewGame,
        SaveGame,
        ReturnToMenu,
        ExitGame
    }

    [Header("Configuración de botones")]
    public List<MenuButton> botones = new List<MenuButton>();

    void Start()
    {
        // Asigna las funciones a cada botón configurado
        foreach (var b in botones)
        {
            if (b.button != null)
                b.button.onClick.AddListener(() => EjecutarAccion(b));
        }

        // Configura el estado inicial del botón de continuar
        ActualizarBotonContinuar();
    }

    void Update()
    {
        // Por si se guarda o borra partida durante ejecución
        ActualizarBotonContinuar();
    }

    void ActualizarBotonContinuar()
    {
        foreach (var b in botones)
        {
            if (b.action == ButtonAction.LoadSavedGame && b.button != null)
            {
                bool tieneGuardado = SaveSystem.HasSave();
                if (b.button.gameObject.activeSelf != tieneGuardado)
                    b.button.gameObject.SetActive(tieneGuardado);
            }
        }
    }

    void EjecutarAccion(MenuButton boton)
    {
        switch (boton.action)
        {
            case ButtonAction.LoadScene:
                if (!string.IsNullOrEmpty(boton.targetScene))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(boton.targetScene);
                }
                break;

            case ButtonAction.LoadSavedGame:
                SaveSystem.LoadGame();
                break;

            case ButtonAction.NewGame:
                SaveSystem.DeleteSave();
                SceneManager.LoadScene("IntroNivel1");
                break;

            case ButtonAction.SaveGame:
                var anxietySystem = FindObjectOfType<AnxietySystem>();
                float ansiedadActual = anxietySystem != null
                    ? anxietySystem.GetType()
                        .GetField("anxietyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(anxietySystem) is float val ? val : 0f
                    : 0f;
                SaveSystem.SaveGame(ansiedadActual, SceneManager.GetActiveScene().name);
                ActualizarBotonContinuar();
                break;

            case ButtonAction.ReturnToMenu:
                Time.timeScale = 1f;
                SceneManager.LoadScene("Menu");
                break;

            case ButtonAction.ExitGame:
                Application.Quit();
                break;

            default:
                Debug.Log("Sin acción asignada al botón: " + boton.buttonName);
                break;
        }
    }
}
