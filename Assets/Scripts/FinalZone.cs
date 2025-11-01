using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

[RequireComponent(typeof(Collider))]
public class FinalZone : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "IntroNivel2"; // nombre de la siguiente escena

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
                Debug.Log("[FinalZone] El jugador tiene la llave. Cargando escena: " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("[FinalZone] El jugador no tiene la llave. No puede salir todavía.");
            }
        }
    }
}
