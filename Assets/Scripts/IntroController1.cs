using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroController1 : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "PrimerNivel"; // escena siguiente

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        // Detecta espacio o Enter (Return)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            SkipIntro();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
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
