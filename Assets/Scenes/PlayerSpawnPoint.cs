using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Objeto vacío que indica el punto de aparición")]
    public Transform spawnPoint;     // Asignar en el Inspector

    void Awake()
    {
        TeleportarJugador();
    }

    private void TeleportarJugador()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[PlayerSpawnPoint] No se encontró un objeto con Tag 'Player'.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[PlayerSpawnPoint] No se asignó ningún SpawnPoint.");
            return;
        }

        // --- DESACTIVAR COSAS QUE MUEVEN AL PLAYER ---
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.applyRootMotion = false;

        // --- TELEPORTE REAL ---
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        // --- REACTIVAR COMPONENTES ---
        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;
        if (anim != null) anim.applyRootMotion = true;

        Debug.Log("[PlayerSpawnPoint] Jugador colocado correctamente en el SpawnPoint.");
    }
}
