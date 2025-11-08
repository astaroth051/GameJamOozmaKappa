using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    public static bool GameIsPaused = false;

    private float dpadCooldown = 0.3f;
    private float nextDpadTime = 0f;

    void Update()
    {
        float dpadY = Input.GetAxis("DPadY"); // Eje vertical del DPad (arriba=1, abajo=-1)

        if (Input.GetKeyDown(KeyCode.Escape) || 
            (dpadY > 0.5f && Time.time > nextDpadTime))
        {
            nextDpadTime = Time.time + dpadCooldown;

            if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void ReturnToMenu()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
