using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIHoverController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonData
    {
        public string name;
        public Button button;
    }

    [Header("Botones controlados (en orden de navegación)")]
    public List<ButtonData> botones = new List<ButtonData>();

    [Header("Efectos visuales")]
    public float scaleOnHover = 1.08f; // solo crecimiento
    public float transitionSpeed = 8f; // suavizado del crecimiento

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private int currentIndex = 0;
    private Vector3[] originalScales;
    private float lastMoveTime;
    private float moveDelay = 0.25f;

    void Start()
    {
        int count = botones.Count;
        originalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            if (botones[i].button == null) continue;

            Button btn = botones[i].button;
            originalScales[i] = btn.transform.localScale;

            int index = i;
            btn.onClick.AddListener(() => OnButtonClick(index));
        }

        // Seleccionar por defecto el botón de "Play" (el primero)
        if (botones.Count > 0 && botones[0].button != null)
        {
            currentIndex = 0;
            EventSystem.current.SetSelectedGameObject(botones[0].button.gameObject);
            PlayHoverSound();
        }
    }

    void Update()
    {
        HandleNavigation();
        UpdateButtonScales();
    }

    void HandleNavigation()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical > 0.5f)
        {
            MoveSelection(-1);
        }
        else if (vertical < -0.5f)
        {
            MoveSelection(1);
        }

        // Confirmar botón
        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            botones[currentIndex].button.onClick.Invoke();
        }

        // Detectar selección por mouse o gamepad
        GameObject current = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < botones.Count; i++)
        {
            if (botones[i].button != null && current == botones[i].button.gameObject && currentIndex != i)
            {
                currentIndex = i;
                PlayHoverSound();
                break;
            }
        }
    }

    void MoveSelection(int direction)
    {
        if (Time.time - lastMoveTime < moveDelay) return;
        lastMoveTime = Time.time;

        currentIndex = Mathf.Clamp(currentIndex + direction, 0, botones.Count - 1);

        if (botones[currentIndex].button != null)
        {
            EventSystem.current.SetSelectedGameObject(botones[currentIndex].button.gameObject);
            PlayHoverSound();
        }
    }

    void UpdateButtonScales()
    {
        for (int i = 0; i < botones.Count; i++)
        {
            if (botones[i].button == null) continue;

            bool isSelected = EventSystem.current.currentSelectedGameObject == botones[i].button.gameObject;
            Vector3 targetScale = isSelected
                ? originalScales[i] * scaleOnHover
                : originalScales[i];

            botones[i].button.transform.localScale =
                Vector3.Lerp(botones[i].button.transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
        }
    }

    void OnButtonClick(int index)
    {
        PlayClickSound();
    }

    void PlayHoverSound()
    {
        if (audioSource == null || hoverSound == null) return;

        // cortar cualquier sonido previo
        audioSource.Stop();
        audioSource.PlayOneShot(hoverSound);
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clickSound);
        }
    }
}
