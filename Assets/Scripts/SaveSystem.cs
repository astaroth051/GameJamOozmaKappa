using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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

        File.WriteAllText(filePath, JsonUtility.ToJson(data, true));

        PlayerPrefs.SetString("LastScene", sceneName);
        PlayerPrefs.Save();

        Debug.Log($"[SaveSystem] Guardado en '{sceneName}', pos {pos}");
    }

    // ----------------------------------------------------
    // CARGAR PARTIDA – ARREGLADO
    // ----------------------------------------------------
    public static void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveSystem] No existe partida guardada.");
            return;
        }

        string json = File.ReadAllText(filePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[SaveSystem] Cargando '{data.sceneName}'...");

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // -------------------------
                // SI HAY UN SPAWNPOINT → NO TELEPORTAR
                // -------------------------
                PlayerSpawnPoint spawn = Object.FindFirstObjectByType<PlayerSpawnPoint>();
                if (spawn != null)
                {
                    Debug.Log("[SaveSystem] SpawnPoint detectado → NO mover jugador.");
                }
                else
                {
                    player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
                    Debug.Log($"[SaveSystem] Posición cargada → {data.posX}, {data.posY}, {data.posZ}");
                }
            }

            // Restaurar ansiedad
            var anxietySys = Object.FindFirstObjectByType<AnxietySystem>();
            if (anxietySys != null)
            {
                var field = typeof(AnxietySystem).GetField("anxietyLevel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                    field.SetValue(anxietySys, data.anxietyLevel);
            }
        };

        SceneManager.LoadScene(data.sceneName);
    }

    public static bool HasSave() => File.Exists(filePath);

    public static void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            PlayerPrefs.DeleteKey("LastScene");
            PlayerPrefs.Save();
        }
    }
}
