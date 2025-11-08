using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AuraCameraSync : MonoBehaviour
{
    public Camera auraCamera; // arrastra aquí la CameraAura en el Inspector

    void LateUpdate()
    {
        if (auraCamera == null) return;
        auraCamera.transform.position = transform.position;
        auraCamera.transform.rotation = transform.rotation;
        auraCamera.fieldOfView = GetComponent<Camera>().fieldOfView;
    }
}
