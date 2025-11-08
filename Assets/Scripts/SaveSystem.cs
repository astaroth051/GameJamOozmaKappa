using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    private const string AnxietyKey = "Anxiety";
    private const string SceneKey = "Scene";
    private const string HasSaveKey = "HasSave";

    // Guarda los datos básicos del juego
    public static void SaveGame(float anxiety, string currentScene)
    {
        PlayerPrefs.SetFloat(AnxietyKey, anxiety);
        PlayerPrefs.SetString(SceneKey, currentScene);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"Juego guardado en escena: {currentScene}, ansiedad: {anxiety}");
    }

    // Verifica si existe partida guardada
    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    // Carga la escena y valores guardados
    public static void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("No hay partida guardada para cargar.");
            return;
        }

        string scene = PlayerPrefs.GetString(SceneKey);
        float anxiety = PlayerPrefs.GetFloat(AnxietyKey);

        Debug.Log($"Cargando partida guardada: {scene}, ansiedad: {anxiety}");
        SceneManager.LoadScene(scene);

        // Si el sistema de ansiedad existe, restaurar valor
        SceneManager.sceneLoaded += (loadedScene, mode) =>
        {
            var anxietySystem = Object.FindObjectOfType<AnxietySystem>();
            if (anxietySystem != null)
            {
                var field = typeof(AnxietySystem).GetField("anxietyLevel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(anxietySystem, anxiety);
                    Debug.Log("Ansiedad restaurada tras cargar la escena.");
                }
            }
        };
    }

    // Borra todos los datos guardados
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Partida eliminada correctamente.");
    }
}
