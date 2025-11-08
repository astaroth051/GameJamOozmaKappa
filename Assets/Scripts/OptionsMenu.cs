using UnityEngine;
using TMPro; // Para TMP_Dropdown

public class OptionsMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        // Cargar dificultad guardada o usar la actual del GameManager
        int savedDifficulty = PlayerPrefs.GetInt("Dificultad", (int)GameManager.Instance.currentDifficulty);
        difficultyDropdown.value = savedDifficulty;

        // Aplicar dificultad inmediatamente
        GameManager.Instance.SetDifficulty(savedDifficulty);
        ApplyDifficulty(savedDifficulty);

        // Guardar automáticamente cada vez que se cambia
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChange);
    }

    public void OnDifficultyChange(int value)
    {
        GameManager.Instance.SetDifficulty(value);
        ApplyDifficulty(value);

        // Guardar configuración automáticamente
        PlayerPrefs.SetInt("Dificultad", value);
        PlayerPrefs.Save();

        Debug.Log("Dificultad cambiada y guardada automáticamente: " + ((GameManager.Difficulty)value).ToString());
    }

    void ApplyDifficulty(int diff)
    {
        // --- Ajustar parámetros de ansiedad ---
        var anxietySystem = FindObjectOfType<AnxietySystem>();
        if (anxietySystem != null)
        {
            switch (diff)
            {
                case 0: // Fácil
                    anxietySystem.anxietyIncreaseRate = 5f;
                    anxietySystem.shadowAnxietyRate = 20f;
                    anxietySystem.maxPillsBeforeOverdose = 5;
                    break;

                case 1: // Normal
                    anxietySystem.anxietyIncreaseRate = 10f;
                    anxietySystem.shadowAnxietyRate = 35f;
                    anxietySystem.maxPillsBeforeOverdose = 3;
                    break;

                case 2: // Difícil
                    anxietySystem.anxietyIncreaseRate = 20f;
                    anxietySystem.shadowAnxietyRate = 50f;
                    anxietySystem.maxPillsBeforeOverdose = 2;
                    break;
            }
        }

        // --- Ajustar parámetros de enemigos (ShadowController) ---
        var sombras = FindObjectsOfType<ShadowController>();
        foreach (var sombra in sombras)
        {
            switch (diff)
            {
                case 0: // Fácil
                    sombra.detectionRadius = 8f;
                    sombra.appearRadius = 10f;
                    sombra.minCooldown = 25f;
                    sombra.maxCooldown = 60f;
                    sombra.reappearDelayMin = 25f;
                    sombra.reappearDelayMax = 50f;
                    break;

                case 1: // Normal
                    sombra.detectionRadius = 12f;
                    sombra.appearRadius = 15f;
                    sombra.minCooldown = 15f;
                    sombra.maxCooldown = 40f;
                    sombra.reappearDelayMin = 20f;
                    sombra.reappearDelayMax = 40f;
                    break;

                case 2: // Difícil
                    sombra.detectionRadius = 16f;
                    sombra.appearRadius = 18f;
                    sombra.minCooldown = 8f;
                    sombra.maxCooldown = 25f;
                    sombra.reappearDelayMin = 15f;
                    sombra.reappearDelayMax = 30f;
                    break;
            }
        }
    }
}
