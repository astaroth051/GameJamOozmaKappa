using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Transform del punto de aparición")]
    public Transform spawnPoint;   // Asignar en el inspector

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("[SpawnPoint] No se encontró un objeto con tag Player.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[SpawnPoint] No asignaste un spawnPoint en el inspector.");
            return;
        }

        // Desactivar CharacterController para evitar errores
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Mover jugador al spawn
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        if (cc != null) cc.enabled = true;

        Debug.Log("[SpawnPoint] Jugador colocado en spawn de la escena.");
    }
}
