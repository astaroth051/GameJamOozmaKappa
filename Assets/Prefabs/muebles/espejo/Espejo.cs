using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Espejo : MonoBehaviour
{
    public RenderTexture texturaEspejo;

    void Start()
    {
        AjustarEscala();
    }

    void AjustarEscala()
    {
        if (texturaEspejo == null) return;

        Renderer rend = GetComponent<Renderer>();
        if (rend == null || rend.material == null) return;

        // Obtiene proporciones del espejo (plano) y la textura
        float proporciónTextura = (float)texturaEspejo.width / texturaEspejo.height;
        float proporciónObjeto = transform.localScale.x / transform.localScale.y;

        Vector2 escala = Vector2.one;
        Vector2 offset = Vector2.zero;

        if (proporciónObjeto > proporciónTextura)
        {
            // El objeto es más ancho → recorta horizontalmente
            float ajuste = proporciónTextura / proporciónObjeto;
            escala.x = ajuste;
            offset.x = (1 - ajuste) / 2;
        }
        else
        {
            // El objeto es más alto → recorta verticalmente
            float ajuste = proporciónObjeto / proporciónTextura;
            escala.y = ajuste;
            offset.y = (1 - ajuste) / 2;
        }

        // Aplica escala y desplazamiento para no deformar la textura
        rend.material.mainTextureScale = escala;
        rend.material.mainTextureOffset = offset;
    }
}
