using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuPrincipal : MonoBehaviour
{
    [System.Serializable]
    public class MenuButton
    {
        public string buttonName;
        public Button button;
        public ButtonAction action;
        public string targetScene;
    }

    public enum ButtonAction
    {
        None,
        LoadScene,       // Cargar escena específica (por targetScene)
        ContinueGame,    // Cargar guardado
        NewGame,         // Empezar desde cero
        Options,         // Ir a opciones
        ExitGame         // Salir
    }

    [Header("Configuración de botones")]
    public List<MenuButton> botones = new List<MenuButton>();

    void Start()
    {
        foreach (var b in botones)
        {
            if (b.button != null)
                b.button.onClick.AddListener(() => EjecutarAccion(b));
        }

        ActualizarBotonContinuar();
    }

    // --------------------------------------------
    // OCULTA O MUESTRA EL BOTÓN “CONTINUAR”
    // --------------------------------------------
    void ActualizarBotonContinuar()
    {
        foreach (var b in botones)
        {
            if (b.action == ButtonAction.ContinueGame && b.button != null)
            {
                bool tieneGuardado = SaveSystem.HasSave();
                b.button.gameObject.SetActive(tieneGuardado);
            }
        }
    }

    // --------------------------------------------
    // ACCIONES DE LOS BOTONES
    // --------------------------------------------
    public void EjecutarAccion(MenuButton boton)
    {
        switch (boton.action)
        {
            // -------------------
            // CARGAR ESCENA MANUAL (si la defines)
            // -------------------
            case ButtonAction.LoadScene:
                if (!string.IsNullOrEmpty(boton.targetScene))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(boton.targetScene);
                }
                break;

            // -------------------
            // CONTINUAR PARTIDA GUARDADA
            // -------------------
            case ButtonAction.ContinueGame:
                if (SaveSystem.HasSave())
                {
                    Time.timeScale = 1f;
                    SaveSystem.LoadGame();
                    Debug.Log("[MenuPrincipal] Cargando partida guardada...");
                }
                else
                {
                    Debug.LogWarning("[MenuPrincipal] No hay partida guardada disponible.");
                }
                break;

            // -------------------
            // NUEVA PARTIDA
            // -------------------
            case ButtonAction.NewGame:
                Time.timeScale = 1f;
                SaveSystem.DeleteSave();
                SceneManager.LoadScene("IntroNivel1");
                Debug.Log("[MenuPrincipal] Nueva partida iniciada desde IntroNivel1.");
                break;

            // -------------------
            // OPCIONES
            // -------------------
            case ButtonAction.Options:
                Time.timeScale = 1f;
                SceneManager.LoadScene("Opciones");
                Debug.Log("[MenuPrincipal] Abriendo opciones.");
                break;

            // -------------------
            // SALIR DEL JUEGO
            // -------------------
            case ButtonAction.ExitGame:
                Application.Quit();
                Debug.Log("[MenuPrincipal] Saliendo del juego...");
                break;
        }
    }
}
