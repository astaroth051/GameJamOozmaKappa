using UnityEngine;
using UnityEngine.SceneManagement;

public class LightingReset : MonoBehaviour
{
    void Awake()
    {
        // Limpia iluminación global y skybox heredado
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.skybox = null;
        DynamicGI.UpdateEnvironment();
    }
}
