using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class OptionsMenu : MonoBehaviour
{
    public Dropdown difficultyDropdown;
    public UniversalRenderPipelineAsset urpAsset;

    void Start()
    {
        difficultyDropdown.value = (int)GameManager.Instance.currentDifficulty;
    }

    public void OnDifficultyChange(int value)
    {
        GameManager.Instance.SetDifficulty(value);
        AdjustAnxietyParameters(value);
    }

    void AdjustAnxietyParameters(int diff)
    {
        var anxietySystem = FindObjectOfType<AnxietySystem>();
        if (anxietySystem == null) return;

        switch (diff)
        {
            case 0: // fácil
                anxietySystem.anxietyIncreaseRate = 5f;
                anxietySystem.shadowAnxietyRate = 20f;
                anxietySystem.maxPillsBeforeOverdose = 5;
                urpAsset.shadowDistance = 50;
                break;
            case 1: // normal
                anxietySystem.anxietyIncreaseRate = 10f;
                anxietySystem.shadowAnxietyRate = 35f;
                anxietySystem.maxPillsBeforeOverdose = 3;
                urpAsset.shadowDistance = 30;
                break;
            case 2: // difícil
                anxietySystem.anxietyIncreaseRate = 20f;
                anxietySystem.shadowAnxietyRate = 50f;
                anxietySystem.maxPillsBeforeOverdose = 2;
                urpAsset.shadowDistance = 15;
                break;
        }
    }
}
