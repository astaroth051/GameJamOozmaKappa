using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Necesario para usar el nuevo Input System

public class IntroController2 : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "SegundoNivel"; // escena siguiente

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        // Teclado: Espacio o Enter
        if ((Keyboard.current != null && 
            (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)))
        {
            Debug.Log("[IntroController] Intro saltada con teclado (Espacio o Enter).");
            SkipIntro();
        }

        // Mando: botón West (X en Xbox o Cuadrado en PlayStation)
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            string controlType = Gamepad.current.displayName;
            Debug.Log($"[IntroController] Intro saltada con mando ({controlType}) usando botón West (X en Xbox o Cuadrado en PlayStation).");
            SkipIntro();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("[IntroController] Video finalizado automáticamente. Cargando siguiente escena...");
        LoadNextScene();
    }

    private void SkipIntro()
    {
        videoPlayer.Stop();
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
