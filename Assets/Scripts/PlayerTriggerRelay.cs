using UnityEngine;

public class PlayerTriggerRelay : MonoBehaviour
{
    private AnxietySystem anxietySystem;

    private void Start()
    {
        anxietySystem = FindObjectOfType<AnxietySystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (anxietySystem != null)
            anxietySystem.NotifyTriggerEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (anxietySystem != null)
            anxietySystem.NotifyTriggerStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (anxietySystem != null)
            anxietySystem.NotifyTriggerExit(other);
    }
}
