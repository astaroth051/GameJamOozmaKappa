using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinalZone : MonoBehaviour
{
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Verificar si la llave fue recogida
            if (KeyItem.llaveRecogida)
            {
                Debug.Log("[FinalZone] El jugador tiene la llave. Cerrando aplicación...");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else
            {
                Debug.Log("[FinalZone] El jugador no tiene la llave. No puede salir todavía.");
            }
        }
    }
}
