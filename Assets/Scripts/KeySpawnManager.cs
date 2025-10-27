using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySpawnManager : MonoBehaviour
{
    [Header("Prefab de la llave")]
    public GameObject llavePrefab;

    [Header("Puntos posibles de aparición")]
    public List<Transform> puntosSpawn = new List<Transform>();

    [Header("Referencia a la sombra")]
    public ShadowController sombra;  // Asigna tu ShadowController aquí

    [Header("Tiempo antes de aparecer tras ser vista")]
    public float delayAparicion = 5f;

    [Header("Audio al recoger la llave")]
    public AudioClip sonidoRecogerLlave;

    private GameObject llaveInstanciada;
    private bool llaveSpawned = false;
    private AudioSource audioSource;

    private void Start()
    {
        if (sombra == null)
        {
            sombra = FindObjectOfType<ShadowController>();
            if (sombra == null)
                Debug.LogWarning("[KeySpawnManager] No se encontró la sombra en la escena.");
        }

        if (puntosSpawn.Count == 0)
            Debug.LogWarning("[KeySpawnManager] No hay puntos de spawn asignados.");

        // Crear AudioSource local (para reproducir el sonido de recogida)
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Verifica si la sombra ya te vio por primera vez
        if (!llaveSpawned && sombra != null && sombra.GetFirstSeen())
        {
            llaveSpawned = true;
            StartCoroutine(SpawnLlaveDespuesDeDelay());
        }
    }

    private IEnumerator SpawnLlaveDespuesDeDelay()
    {
        yield return new WaitForSeconds(delayAparicion);

        if (llavePrefab == null || puntosSpawn.Count == 0)
        {
            Debug.LogWarning("[KeySpawnManager] No se puede spawnear la llave: faltan referencias.");
            yield break;
        }

        // Selecciona un punto aleatorio
        Transform punto = puntosSpawn[Random.Range(0, puntosSpawn.Count)];

        // Instancia la llave
        llaveInstanciada = Instantiate(llavePrefab, punto.position, punto.rotation);

        // Agrega un collider si no tiene (para detectar colisión con el jugador)
        Collider col = llaveInstanciada.GetComponent<Collider>();
        if (col == null)
            col = llaveInstanciada.AddComponent<BoxCollider>();
        col.isTrigger = true;

        // Log detallado en consola
        Debug.Log($"[KeySpawnManager] 🔑 Llave generada en: {punto.name}\n" +
                  $"→ Posición: {punto.position}\n" +
                  $"→ Tiempo: {Time.time:F2} segundos desde el inicio.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cuando el jugador entre en contacto con el área final
        if (other.CompareTag("Player"))
        {
            GameObject finalObj = GameObject.FindGameObjectWithTag("FinalPiso1");

            if (finalObj != null && other.bounds.Intersects(finalObj.GetComponent<Collider>().bounds))
            {
                Debug.Log("El jugador alcanzó el área final (FinalPiso1). Cerrando aplicación...");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }

    // --- NUEVO BLOQUE ---
    private void OnTriggerStay(Collider other)
    {
        // Detecta si el jugador toca la llave (que tiene Collider tipo trigger)
        if (llaveInstanciada != null && other.CompareTag("Player"))
        {
            Collider llaveCol = llaveInstanciada.GetComponent<Collider>();
            if (llaveCol != null && other.bounds.Intersects(llaveCol.bounds))
            {
                Debug.Log("[KeySpawnManager] Jugador recogió la llave.");

                if (sonidoRecogerLlave != null)
                {
                    audioSource.clip = sonidoRecogerLlave;
                    audioSource.Play();
                    Debug.Log("[KeySpawnManager] Sonido de llave reproducido.");
                }

                // Oculta la llave visualmente mientras suena
                foreach (var rend in llaveInstanciada.GetComponentsInChildren<Renderer>())
                    rend.enabled = false;

                // Destruye la llave después del sonido (si lo hay)
                float delay = (sonidoRecogerLlave != null) ? sonidoRecogerLlave.length : 0f;
                Destroy(llaveInstanciada, delay);

                llaveInstanciada = null; // evita dobles ejecuciones
            }
        }
    }
}
