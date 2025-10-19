using UnityEngine;

public class TankCameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;                      // El jugador
    public Vector3 offset = new Vector3(0, 1.5f, -1.8f);

    [Header("Ajustes de cámara")]
    public float smoothPosition = 8f;             // Suavizado de movimiento
    public float smoothRotation = 6f;             // Suavizado de rotación
    [Range(-40, 40)] public float tiltAngle = 10f;
    public float stability = 0.95f;               // Filtro anti-bamboleo (0.9–0.99)

    private Vector3 smoothedTargetPos;
    private float smoothedYaw;

    private void LateUpdate()
    {
        if (!target) return;

        // --- FILTRO DE ESTABILIDAD ---
        // Suaviza posición global del target (elimina vibraciones de animación)
        smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, target.position, 1 - stability);

        // Calcula solo el eje Y del jugador (ignora inclinaciones del rig)
        float targetYaw = target.rotation.eulerAngles.y;
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, Time.deltaTime * smoothRotation);

        // --- POSICIÓN ---
        Quaternion yawRotation = Quaternion.Euler(0, smoothedYaw, 0);
        Vector3 desiredPosition = smoothedTargetPos + yawRotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothPosition);

        // --- ROTACIÓN ---
        Quaternion desiredRotation = Quaternion.Euler(tiltAngle, smoothedYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * smoothRotation);
    }

    private void Start()
    {
        // Inicializa para evitar salto inicial
        smoothedTargetPos = target.position;
        smoothedYaw = target.rotation.eulerAngles.y;
    }
}
