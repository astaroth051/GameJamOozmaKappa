using UnityEngine;

public class MirrorCameraFollow : MonoBehaviour
{
    public Transform mainCamera;
    public Transform mirrorCamera;
    public Transform mirrorPlane;

    void LateUpdate()
    {
        // Calcula la posición reflejada
        Vector3 toCam = mainCamera.position - mirrorPlane.position;
        Vector3 reflectedPos = Vector3.Reflect(toCam, mirrorPlane.forward);
        mirrorCamera.position = mirrorPlane.position + reflectedPos;

        // Calcula la dirección reflejada (corregida)
        Vector3 reflectedDir = Vector3.Reflect(-mainCamera.forward, mirrorPlane.forward);
        mirrorCamera.rotation = Quaternion.LookRotation(reflectedDir, Vector3.up);
    }
}
