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
    public float movimientoMinimo = 0.5f; // Sensibilidad: movimiento mínimo para mover la cámara

    private Vector3 smoothedTargetPos;
    private float smoothedYaw;
    private Vector3 ultimaPosicion;

    private void Start()
    {
        smoothedTargetPos = target.position;
        smoothedYaw = target.rotation.eulerAngles.y;
        ultimaPosicion = target.position;
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Detecta desplazamiento real en XZ
        Vector3 delta = target.position - ultimaPosicion;
        Vector2 planoXZ = new Vector2(delta.x, delta.z);
        float velocidadPlano = planoXZ.magnitude / Time.deltaTime;

        // Si el movimiento lateral es muy pequeño, ignóralo (fija XZ)
        Vector3 referenciaPos = smoothedTargetPos;
        if (velocidadPlano > movimientoMinimo)
        {
            smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, target.position, 1 - stability);
        }
        else
        {
            smoothedTargetPos.y = Mathf.Lerp(smoothedTargetPos.y, target.position.y, 1 - stability);
        }

        ultimaPosicion = target.position;

        float targetYaw = target.rotation.eulerAngles.y;
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, Time.deltaTime * smoothRotation);
        Quaternion yawRotation = Quaternion.Euler(0, smoothedYaw, 0);

        Vector3 desiredPosition = smoothedTargetPos + yawRotation * offset;
        desiredPosition.y = target.position.y + offset.y;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothPosition);
        Quaternion desiredRotation = Quaternion.Euler(tiltAngle, smoothedYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * smoothRotation);
    }
}
