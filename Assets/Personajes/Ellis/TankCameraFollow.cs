using UnityEngine;

public class TankCameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;                      // El jugador
    public Vector3 offset = new Vector3(0, 1.5f, -1.8f);

    [Header("Ajustes de cámara")]
    public float smoothPosition = 8f;             // Suavizado de movimiento
    public float smoothRotation = 6f;             // Suavizado base de rotación
    [Range(-40, 40)] public float tiltAngle = 10f;
    public float stability = 0.95f;               // Filtro anti-bamboleo (0.9–0.99)
    public float runRotationDamping = 0.5f;       // Factor para reducir el giro al correr (0–1)
    public float maxTiltAtRun = 5f;               // Inclinación reducida al correr

    private Vector3 smoothedTargetPos;
    private float smoothedYaw;
    private float targetSpeed;
    private Rigidbody targetRb;                   // opcional si el player usa Rigidbody

    private void Start()
    {
        smoothedTargetPos = target.position;
        smoothedYaw = target.rotation.eulerAngles.y;
        targetRb = target.GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (!target) return;

        // --- FILTRO DE ESTABILIDAD ---
        smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, target.position, 1 - stability);

        // Calcula yaw del jugador
        float targetYaw = target.rotation.eulerAngles.y;

        // Detecta velocidad del objetivo (si tiene Rigidbody o por delta de posición)
        if (targetRb)
            targetSpeed = targetRb.velocity.magnitude;
        else
            targetSpeed = (target.position - smoothedTargetPos).magnitude / Time.deltaTime;

        // Ajusta suavizado según si va corriendo
        float rotationLerp = smoothRotation;
        float currentTilt = tiltAngle;

        if (targetSpeed > 3f) // umbral típico de correr
        {
            rotationLerp *= runRotationDamping; // gira más suave
            currentTilt = Mathf.Lerp(tiltAngle, maxTiltAtRun, 0.5f);
        }

        // --- ROTACIÓN Y POSICIÓN ---
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, Time.deltaTime * rotationLerp);
        Quaternion yawRotation = Quaternion.Euler(0, smoothedYaw, 0);
        Vector3 desiredPosition = smoothedTargetPos + yawRotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothPosition);
        Quaternion desiredRotation = Quaternion.Euler(currentTilt, smoothedYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerp);
    }
}
