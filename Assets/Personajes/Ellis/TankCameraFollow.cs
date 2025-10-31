using UnityEngine;

public class TankCameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; 
    public Vector3 offset = new Vector3(0, 1.5f, -1.8f);

    [Header("Ajustes de cámara")]
    public float smoothPosition = 8f;
    public float smoothRotation = 6f;
    [Range(-40, 40)] public float tiltAngle = 10f;
    public float stability = 0.95f;
    public float movimientoMinimo = 0.5f;

    [Header("Colisión de cámara")]
    public float radioColision = 0.3f;
    public float distanciaMinima = 0.8f;  // más segura para no acercar tanto
    public float alturaMinima = 0.5f;     // límite para no bajar demasiado
    public LayerMask capasColision;

    private Vector3 smoothedTargetPos;
    private float smoothedYaw;
    private Vector3 ultimaPosicion;
    private float distanciaActual;

    void Start()
    {
        if (!target)
        {
            Debug.LogError("[TankCameraFollow] No hay objetivo asignado a la cámara.");
            enabled = false;
            return;
        }

        smoothedTargetPos = target.position;
        smoothedYaw = target.rotation.eulerAngles.y;
        ultimaPosicion = target.position;
        distanciaActual = offset.magnitude;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Movimiento del objetivo
        Vector3 delta = target.position - ultimaPosicion;
        Vector2 planoXZ = new Vector2(delta.x, delta.z);
        float velocidadPlano = planoXZ.magnitude / Time.deltaTime;

        if (velocidadPlano > movimientoMinimo)
            smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, target.position, 1 - stability);
        else
            smoothedTargetPos.y = Mathf.Lerp(smoothedTargetPos.y, target.position.y, 1 - stability);

        ultimaPosicion = target.position;

        // Rotación suave
        float targetYaw = target.rotation.eulerAngles.y;
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, Time.deltaTime * smoothRotation);
        Quaternion yawRotation = Quaternion.Euler(0, smoothedYaw, 0);

        // Posición ideal de la cámara
        Vector3 desiredPosition = smoothedTargetPos + yawRotation * offset;

        // Detección de colisión
        Vector3 direction = (desiredPosition - target.position).normalized;
        float desiredDistance = offset.magnitude;

        if (Physics.SphereCast(target.position, radioColision, direction, out RaycastHit hit, desiredDistance, capasColision))
        {
            distanciaActual = Mathf.Clamp(hit.distance * 0.9f, distanciaMinima, desiredDistance);
        }
        else
        {
            distanciaActual = Mathf.Lerp(distanciaActual, desiredDistance, Time.deltaTime * smoothPosition);
        }

        // Aplica la distancia ajustada
        desiredPosition = target.position + direction * distanciaActual;

        // --- CORRECCIÓN VERTICAL ---
        // Evita que la cámara baje demasiado al acercarse
        float alturaDeseada = target.position.y + offset.y;
        if (desiredPosition.y < alturaDeseada - alturaMinima)
            desiredPosition.y = alturaDeseada - alturaMinima;
        // ----------------------------

        // Suavizado de movimiento y rotación
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothPosition);
        Quaternion desiredRotation = Quaternion.Euler(tiltAngle, smoothedYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * smoothRotation);
    }
}
