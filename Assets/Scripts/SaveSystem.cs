using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Sistema simple de guardado basado en JSON + PlayerPrefs.
/// Guarda: nombre de escena, posición del jugador, nivel de ansiedad.
/// </summary>
public static class SaveSystem
{
    private static string filePath = Application.persistentDataPath + "/save.json";

    [System.Serializable]
    private class SaveData
    {
        public string sceneName;
        public float posX, posY, posZ;
        public float anxietyLevel;
    }

    // -------------------------------
    // GUARDAR PARTIDA
    // -------------------------------
    public static void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SaveSystem] No se encontró al jugador para guardar.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        Vector3 pos = player.transform.position;

        float anxiety = 0f;
        var anxietySys = Object.FindFirstObjectByType<AnxietySystem>();
        if (anxietySys != null)
        {
            var field = typeof(AnxietySystem).GetField("anxietyLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                anxiety = (float)field.GetValue(anxietySys);
        }

        SaveData data = new SaveData()
        {
            sceneName = sceneName,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z,
            anxietyLevel = anxiety
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        PlayerPrefs.SetString("LastScene", sceneName);
        PlayerPrefs.Save();

        Debug.Log($"[SaveSystem] Partida guardada en '{sceneName}' posición ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
    }

    // -------------------------------
    // CARGAR PARTIDA
    // -------------------------------
    public static void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveSystem] No existe partida guardada.");
            return;
        }

        string json = File.ReadAllText(filePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[SaveSystem] Cargando partida desde '{data.sceneName}'...");


        SceneManager.sceneLoaded += (scene, mode) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
                Debug.Log($"[SaveSystem] Posición restaurada a ({data.posX:F1}, {data.posY:F1}, {data.posZ:F1})");
            }

            var anxietySys = Object.FindFirstObjectByType<AnxietySystem>();
            if (anxietySys != null)
            {
                var field = typeof(AnxietySystem).GetField("anxietyLevel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(anxietySys, data.anxietyLevel);
                    Debug.Log($"[SaveSystem] Nivel de ansiedad restaurado a {data.anxietyLevel:F1}");
                }
            }
        };

        // Cargar finalmente la escena guardada
        SceneManager.LoadScene(data.sceneName);
    }


    // -------------------------------
    // COMPROBAR / BORRAR PARTIDA
    // -------------------------------
    public static bool HasSave()
    {
        return File.Exists(filePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            PlayerPrefs.DeleteKey("LastScene");
            PlayerPrefs.Save();
            Debug.Log("[SaveSystem] Partida eliminada correctamente.");
        }
    }
}
