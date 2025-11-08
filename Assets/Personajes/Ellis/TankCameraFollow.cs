using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class TankCameraFollow : MonoBehaviour
{
    public enum CameraMode { ThirdPerson, FirstPerson }

    [Header("Modo de cámara")]
    public CameraMode cameraMode = CameraMode.ThirdPerson;
    public Transform firstPersonPoint;
    public float transitionSpeed = 5f;

    [Header("Objetivo (tercera persona)")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, -1.8f);

    [Header("Ajustes de cámara")]
    public float smoothPosition = 8f;
    public float smoothRotation = 6f;
    [Range(-40, 40)] public float tiltAngle = 10f;
    public float stability = 0.95f;
    public float movimientoMinimo = 0.5f;

    [Header("Colisión de cámara (solo tercera persona)")]
    public float radioColision = 0.3f;
    public float distanciaMinima = 0.8f;
    public float alturaMinima = 0.5f;
    public LayerMask capasColision;

    [Header("Post-Procesado / Excepción")]
    public Volume globalVolume;
    public LayerMask ignorePostFXLayers;
    public float detectionRadius = 3f;
    public bool debugDetection = false;
    public float transitionTime = 3f;

    private Vector3 smoothedTargetPos;
    private float smoothedYaw;
    private Vector3 ultimaPosicion;
    private float distanciaActual;

    private float currentVelocity;
    private bool allowPostFXLogic = false;
    [HideInInspector] public bool freezeCamera = false; // ★ agregado para FuneralSequence

    private void Start()
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

        if (globalVolume != null)
            globalVolume.weight = 1f;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "CuartoNivel")
        {
            allowPostFXLogic = true;
            if (debugDetection) Debug.Log("PostFX dinámico activado: escena CuartoNivel");
        }
    }

    private void LateUpdate()
    {
        if (!target) return;

        // ★ agregado para FuneralSequence (congelar sin romper nada)
        if (freezeCamera)
        {
            if (allowPostFXLogic)
                CheckPostFXExclusion();
            return;
        }

        if (cameraMode == CameraMode.ThirdPerson)
            UpdateThirdPerson();
        else
            UpdateFirstPerson();

        if (allowPostFXLogic)
            CheckPostFXExclusion();
    }

    private void UpdateThirdPerson()
    {
        Vector3 delta = target.position - ultimaPosicion;
        Vector2 planoXZ = new Vector2(delta.x, delta.z);
        float velocidadPlano = planoXZ.magnitude / Time.deltaTime;

        if (velocidadPlano > movimientoMinimo)
            smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, target.position, 1 - stability);
        else
            smoothedTargetPos.y = Mathf.Lerp(smoothedTargetPos.y, target.position.y, 1 - stability);

        ultimaPosicion = target.position;

        float targetYaw = target.rotation.eulerAngles.y;
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, Time.deltaTime * smoothRotation);
        Quaternion yawRotation = Quaternion.Euler(0, smoothedYaw, 0);

        Vector3 desiredPosition = smoothedTargetPos + yawRotation * offset;
        Vector3 direction = (desiredPosition - target.position).normalized;
        float desiredDistance = offset.magnitude;

        if (Physics.SphereCast(target.position, radioColision, direction, out RaycastHit hit, desiredDistance, capasColision))
            distanciaActual = Mathf.Clamp(hit.distance * 0.9f, distanciaMinima, desiredDistance);
        else
            distanciaActual = Mathf.Lerp(distanciaActual, desiredDistance, Time.deltaTime * smoothPosition);

        desiredPosition = target.position + direction * distanciaActual;

        float alturaDeseada = target.position.y + offset.y;
        if (desiredPosition.y < alturaDeseada - alturaMinima)
            desiredPosition.y = alturaDeseada - alturaMinima;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothPosition);
        Quaternion desiredRotation = Quaternion.Euler(tiltAngle, smoothedYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * smoothRotation);
    }

    private void UpdateFirstPerson()
    {
        if (firstPersonPoint == null)
        {
            Debug.LogWarning("[TankCameraFollow] No se ha asignado el punto de vista de primera persona.");
            return;
        }

        transform.position = Vector3.Lerp(transform.position, firstPersonPoint.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, firstPersonPoint.rotation, Time.deltaTime * transitionSpeed);
    }

    // --- Detección suave solo si estamos en CuartoNivel ---
    private void CheckPostFXExclusion()
    {
        if (globalVolume == null) return;

        GameObject[] ignoredObjects = FindObjectsOfType<GameObject>();
        bool foundIgnoreObject = false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());

        foreach (GameObject obj in ignoredObjects)
        {
            if (!obj.activeInHierarchy) continue;
            if (((1 << obj.layer) & ignorePostFXLayers) == 0) continue;

            Renderer r = obj.GetComponent<Renderer>();
            if (r == null) continue;

            if (!GeometryUtility.TestPlanesAABB(planes, r.bounds)) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= detectionRadius)
            {
                foundIgnoreObject = true;
                break;
            }
        }

        float target = foundIgnoreObject ? 0f : 1f;

        // Suavizado (≈3 s)
        globalVolume.weight = Mathf.SmoothDamp(globalVolume.weight, target, ref currentVelocity, transitionTime);

        if (debugDetection)
        {

        }
    }

    // Cambiar modo de cámara desde UI o código
    public void SetCameraMode(CameraMode mode)
    {
        cameraMode = mode;
    }

    // Gizmo visual de detección
    private void OnDrawGizmosSelected()
    {
        if (!debugDetection) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
