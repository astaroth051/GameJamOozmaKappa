using UnityEngine;

public class TankCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, -1.8f);
    public float smoothPosition = 8f;
    public float smoothRotation = 6f;
    public float stability = 0.95f;
    public float runRotationDamping = 0.5f;
    public float tiltAngle = 10f;
    public float maxTiltAtRun = 5f;

    private Vector3 smoothedTargetPos;
    private float smoothedYaw;
    private float targetSpeed;
    private Rigidbody targetRb;

    private void Start()
    {
        smoothedTargetPos = target.position;
        smoothedYaw = target.rotation.eulerAngles.y;
        targetRb = target.GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (!target) return;

        smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, target.position, 1 - stability);
        float targetYaw = target.rotation.eulerAngles.y;

        if (targetRb)
            targetSpeed = targetRb.velocity.magnitude;
        else
            targetSpeed = (target.position - smoothedTargetPos).magnitude / Time.deltaTime;

        float rotationLerp = smoothRotation;
        float currentTilt = tiltAngle;

        if (targetSpeed > 3f)
        {
            rotationLerp *= runRotationDamping;
            currentTilt = Mathf.Lerp(tiltAngle, maxTiltAtRun, 0.5f);
        }

        smoothedYaw = Mathf.LerpAngle(smoothedYaw, targetYaw, Time.deltaTime * rotationLerp);
        Quaternion yawRotation = Quaternion.Euler(0, smoothedYaw, 0);
        Vector3 desiredPosition = smoothedTargetPos + yawRotation * offset;

        desiredPosition.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothPosition);
        Quaternion desiredRotation = Quaternion.Euler(0, smoothedYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerp);
    }
}
