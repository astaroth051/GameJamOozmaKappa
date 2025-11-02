using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DoorInteractable : MonoBehaviour
{
    [Header("Configuración de la puerta")]
    public Transform puerta;             
    public float anguloApertura = 90f;   
    public float velocidadApertura = 2f; 
    public bool abrirEnZ = false;        
    private bool abierta = false;
    public bool interactuable = false;   

    [Header("Control del jugador")]
    public MonoBehaviour scriptMovimientoJugador; // Asigna aquí EllisTankController

    [Header("UI de interacción")]
    public Canvas canvasInteraccion;     
    public TextMeshProUGUI textoUI;      
    public string mensajeTeclado = "Presiona E para abrir";
    public string mensajeGamepad = "Presiona ○ para abrir";

    private bool jugadorEnZona = false;
    private Quaternion rotacionInicial;
    private Quaternion rotacionFinal;
    private Camera camaraJugador;

    private void Start()
    {
        if (puerta == null)
            puerta = transform;

        rotacionInicial = puerta.localRotation;

        float rotZ = abrirEnZ ? anguloApertura : 0f;
        float rotY = abrirEnZ ? 0f : anguloApertura;

        rotacionFinal = Quaternion.Euler(
            puerta.localEulerAngles.x,
            puerta.localEulerAngles.y + rotY,
            puerta.localEulerAngles.z + rotZ
        );

        if (canvasInteraccion != null)
            canvasInteraccion.gameObject.SetActive(false);

        camaraJugador = Camera.main;

        // Si la puerta aún no es interactuable, bloquear movimiento del jugador
        if (!interactuable && scriptMovimientoJugador != null)
        {
            scriptMovimientoJugador.enabled = false;
            Debug.Log("[DoorInteractable] Movimiento del jugador desactivado durante fade.");
        }
    }

    private void Update()
    {
        // Si la puerta se vuelve interactuable (desde FadeAndTrigger), reactivar movimiento
        if (interactuable && scriptMovimientoJugador != null && !scriptMovimientoJugador.enabled)
        {
            scriptMovimientoJugador.enabled = true;
            Debug.Log("[DoorInteractable] Movimiento del jugador reactivado tras fade.");
        }

        if (jugadorEnZona && interactuable && !abierta)
        {
            bool teclaE = false;
            bool botonGamepad = false;

            // --- Entrada por teclado ---
            if (Input.GetKeyDown(KeyCode.E))
            {
                teclaE = true;
                Debug.Log("[DoorInteractable] Tecla E presionada dentro del área interactuable.");
            }

            // --- Entrada por mando (B o círculo) ---
            if (Input.GetButtonDown("Fire2"))
            {
                botonGamepad = true;
                Debug.Log("[DoorInteractable] Botón rojo (Fire2) presionado dentro del área interactuable.");
            }

            if (teclaE || botonGamepad)
            {
                Debug.Log("[DoorInteractable] Interacción detectada → Abriendo puerta.");
                StartCoroutine(AbrirPuerta());
            }
        }

        // Canvas sigue mirando a la cámara
        if (canvasInteraccion != null && canvasInteraccion.gameObject.activeSelf && camaraJugador != null)
        {
            canvasInteraccion.transform.LookAt(camaraJugador.transform);
            canvasInteraccion.transform.Rotate(0, 180f, 0);
        }
    }

    private IEnumerator AbrirPuerta()
    {
        abierta = true;

        float t = 0f;
        Quaternion inicio = puerta.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * velocidadApertura;
            puerta.localRotation = Quaternion.Slerp(inicio, rotacionFinal, t);
            yield return null;
        }

        if (canvasInteraccion != null)
            canvasInteraccion.gameObject.SetActive(false);

        Debug.Log("[DoorInteractable] Puerta abierta.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnZona = true;

        if (canvasInteraccion != null)
        {
            canvasInteraccion.gameObject.SetActive(true);
            textoUI.text = Gamepad.current != null ? mensajeGamepad : mensajeTeclado;
        }

        Debug.Log("[DoorInteractable] Jugador dentro del área de interacción.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorEnZona = false;

        if (canvasInteraccion != null)
            canvasInteraccion.gameObject.SetActive(false);

        Debug.Log("[DoorInteractable] Jugador salió del área de interacción.");
    }
}
