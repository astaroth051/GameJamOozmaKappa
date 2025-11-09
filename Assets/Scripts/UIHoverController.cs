using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UIHoverController : MonoBehaviour
{
    [System.Serializable]
    public class UIElementData
    {
        public string name;
        public Selectable selectable;   // Puede ser Button, Dropdown, Slider, etc.
        public Image image;             // Imagen asociada (asignar en el inspector)
        public bool isDropdown;         // Marcar si es un Dropdown
    }

    [Header("Elementos en orden de navegación")]
    public List<UIElementData> elementos = new List<UIElementData>();

    [Header("Efectos visuales")]
    public float scaleOnHover = 1.08f;
    public float transitionSpeed = 8f;
    public Color32 selectedColor = new Color32(255, 101, 0, 255); // FF6500
    public Color32 normalColor = new Color32(255, 255, 255, 255);
    public float selectedAlpha = 1f;
    public float normalAlpha = 0.55f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private int currentIndex = 0;
    private Vector3[] originalScales;
    private Color32[] originalColors;
    private float lastMoveTime;
    private float moveDelay = 0.25f;
    private bool initialized = false;
    private bool dropdownOpen = false;

    void Start()
    {
        InicializarElementos();
    }

    void Update()
    {
        if (!initialized || EventSystem.current.currentSelectedGameObject == null)
            InicializarElementos();

        // Si hay un dropdown abierto, no interferir
        if (dropdownOpen)
        {
            if (GameObject.Find("Dropdown List") == null)
                dropdownOpen = false; // se cerró el dropdown
            return;
        }

        HandleNavigation();
        UpdateVisuals();
    }

    void InicializarElementos()
    {
        int count = elementos.Count;
        originalScales = new Vector3[count];
        originalColors = new Color32[count];

        for (int i = 0; i < count; i++)
        {
            var sel = elementos[i].selectable;
            if (!sel) continue;

            originalScales[i] = sel.transform.localScale;
            if (elementos[i].image != null)
            {
                originalColors[i] = elementos[i].image.color;
                elementos[i].image.raycastTarget = false;
            }

            // Solo agregamos efecto de click (sin borrar otros listeners)
            if (sel is Button btn)
            {
                int index = i;
                btn.onClick.AddListener(() => PlayClickSound());
            }
            else if (sel is Dropdown dropdown)
            {
                int index = i;
                dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(index); });
            }
        }

        // Selecciona el primer elemento activo
        for (int i = 0; i < elementos.Count; i++)
        {
            if (elementos[i].selectable && elementos[i].selectable.gameObject.activeInHierarchy)
            {
                currentIndex = i;
                EventSystem.current.SetSelectedGameObject(elementos[i].selectable.gameObject);
                PlayHoverSound();
                break;
            }
        }

        initialized = true;
    }

    void HandleNavigation()
    {
        float vertical = 0f;
        try { vertical = Input.GetAxisRaw("Vertical"); } catch { vertical = 0f; }

        if (vertical > 0.5f) MoveSelection(-1);
        else if (vertical < -0.5f) MoveSelection(1);

        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            var actual = elementos[currentIndex];
            if (actual.selectable == null) return;

            if (actual.isDropdown && actual.selectable is Dropdown dropdown)
            {
                dropdown.Show();
                dropdownOpen = true;
                PlayClickSound();
                StartCoroutine(SeleccionarPrimerItemDropdown());
            }
            else if (actual.selectable is Button button)
            {
                // Unity ejecutará el onClick ya asignado por MenuPrincipal
                button.onClick.Invoke();
                PlayClickSound();
            }
        }

        // Detectar cambio de selección
        GameObject current = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < elementos.Count; i++)
        {
            if (elementos[i].selectable && current == elementos[i].selectable.gameObject && currentIndex != i)
            {
                currentIndex = i;
                PlayHoverSound();
                break;
            }
        }
    }

    void MoveSelection(int direction)
    {
        if (Time.unscaledTime - lastMoveTime < moveDelay) return;
        lastMoveTime = Time.unscaledTime;

        currentIndex = Mathf.Clamp(currentIndex + direction, 0, elementos.Count - 1);
        if (elementos[currentIndex].selectable && elementos[currentIndex].selectable.gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(elementos[currentIndex].selectable.gameObject);
            PlayHoverSound();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < elementos.Count; i++)
        {
            var sel = elementos[i].selectable;
            var img = elementos[i].image;
            if (!sel) continue;

            bool selected = EventSystem.current.currentSelectedGameObject == sel.gameObject;

            Vector3 targetScale = selected ? originalScales[i] * scaleOnHover : originalScales[i];
            sel.transform.localScale = Vector3.Lerp(sel.transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);

            if (img != null)
            {
                Color32 baseColor = selected ? selectedColor : normalColor;
                float alpha = selected ? selectedAlpha : normalAlpha;
                Color finalColor = new Color(baseColor.r / 255f, baseColor.g / 255f, baseColor.b / 255f, alpha);
                img.color = finalColor;
            }
        }
    }

    IEnumerator SeleccionarPrimerItemDropdown()
    {
        yield return null;

        GameObject dropdownList = GameObject.Find("Dropdown List");
        if (dropdownList == null) yield break;

        Toggle firstToggle = dropdownList.GetComponentInChildren<Toggle>();
        if (firstToggle != null)
        {
            EventSystem.current.SetSelectedGameObject(firstToggle.gameObject);
        }
    }

    void OnDropdownValueChanged(int index)
    {
        PlayClickSound();
    }

    void PlayHoverSound()
    {
        if (!audioSource || !hoverSound) return;
        audioSource.Stop();
        audioSource.PlayOneShot(hoverSound);
    }

    void PlayClickSound()
    {
        if (!audioSource || !clickSound) return;
        audioSource.Stop();
        audioSource.PlayOneShot(clickSound);
    }
}
