using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuPrincipal_Jugable : MonoBehaviour
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
        ContinueGame,   // Reanudar
        SaveGame,       // Guardar progreso
        Options,        // Escena Opciones
        ReturnToMenu,   // Volver al menú principal
        ExitGame        // Salir del juego
    }

    [Header("Botones del menú de pausa")]
    public List<MenuButton> botones = new List<MenuButton>();

    void Start()
    {
        foreach (var b in botones)
        {
            if (b.button != null)
                b.button.onClick.AddListener(() => EjecutarAccion(b));
        }
    }

    // -------------------------------------------------
    // ACCIONES DE LOS BOTONES
    // -------------------------------------------------
    public void EjecutarAccion(MenuButton boton)
    {
        switch (boton.action)
        {
            // -------------------
            // CONTINUAR JUEGO
            // -------------------
            case ButtonAction.ContinueGame:
                Time.timeScale = 1f;
                var pauseCtrl = Object.FindFirstObjectByType<PauseController>();
                if (pauseCtrl != null)
                {
                    pauseCtrl.Reanudar();
                    Debug.Log("[MenuPrincipal_Jugable] Continuar: reanudando el juego.");
                }
                else
                    Debug.LogWarning("[MenuPrincipal_Jugable] No se encontró PauseController.");
                break;

            // -------------------
            // GUARDAR PARTIDA
            // -------------------
            case ButtonAction.SaveGame:
                Time.timeScale = 1f; // no es estrictamente necesario, pero por seguridad
                SaveSystem.SaveGame();
                Debug.Log("[MenuPrincipal_Jugable] Partida guardada correctamente.");
                break;

            // -------------------
            // OPCIONES
            // -------------------
            case ButtonAction.Options:
                Time.timeScale = 1f;
                SceneManager.LoadScene("Opciones");
                Debug.Log("[MenuPrincipal_Jugable] Abriendo Opciones.");
                break;

            // -------------------
            // VOLVER AL MENÚ PRINCIPAL
            // -------------------
            case ButtonAction.ReturnToMenu:
                Time.timeScale = 1f;
                SceneManager.LoadScene("Menu");
                Debug.Log("[MenuPrincipal_Jugable] Volviendo al menú principal.");
                break;

            // -------------------
            // SALIR DEL JUEGO
            // -------------------
            case ButtonAction.ExitGame:
                Application.Quit();
                Debug.Log("[MenuPrincipal_Jugable] Cerrando el juego.");
                break;
        }
    }
}
