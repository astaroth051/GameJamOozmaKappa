using UnityEngine;

public class SkyboxSetter : MonoBehaviour
{
    [Header("Asigna aquí tu material de skybox (HDRI o panorámico)")]
    public Material customSkybox;

    void Start()
    {
        if (customSkybox != null)
        {
            // Asigna el material como skybox global
            RenderSettings.skybox = customSkybox;

            // Fuerza a Unity a recalcular el entorno y reflejos
            DynamicGI.UpdateEnvironment();

        }
        else
        {
        }
    }
}
