using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("Audio cuando el jugador ve a la sombra (momento poético)")]
    public AudioClip sonidoMomentoSombra;

    [Header("Texto TMP en pantalla")]
    public TextMeshProUGUI textoUI; // Asigna el TextMeshProUGUI del Canvas por el inspector

    private GameObject llaveInstanciada;
    private bool llaveSpawned = false;
    private bool momentoSombraMostrado = false;
    private AudioSource audioSource;

    private void Start()
    {
        // Buscar sombra automáticamente si no está asignada
        if (sombra == null)
        {
            sombra = FindObjectOfType<ShadowController>();
            if (sombra == null)
                Debug.LogWarning("[KeySpawnManager] No se encontró la sombra en la escena.");
        }

        // Verificar puntos de spawn
        if (puntosSpawn.Count == 0)
            Debug.LogWarning("[KeySpawnManager] No hay puntos de spawn asignados.");

        // Crear fuente de audio local
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        if (textoUI != null)
        {
            textoUI.text = "";
            textoUI.alpha = 0f;
        }
    }

    private void Update()
    {
        if (sombra == null) return;

        // Cuando el jugador ve a la sombra por primera vez
        if (!momentoSombraMostrado && sombra.GetFirstSeen())
        {
            momentoSombraMostrado = true;
            StartCoroutine(MostrarMomentoSombra());
        }

        // Luego del mensaje, spawnear la llave
        if (momentoSombraMostrado && !llaveSpawned && sombra.GetFirstSeen())
        {
            llaveSpawned = true;
            StartCoroutine(SpawnLlaveDespuesDeDelay());
        }
    }

    // -------------------------------------------------------
    // MOMENTO POÉTICO
    // -------------------------------------------------------
    private IEnumerator MostrarMomentoSombra()
    {
        Debug.Log("[KeySpawnManager] 🕯️ Momento poético: la sombra fue vista.");

        // Atenuar todo el audio del juego
        AudioListener.volume = 0.05f;

        // Texto poético
        if (textoUI != null)
        {
            textoUI.text = "La sombra dejó de ocultarse... la viste a los ojos, y la llave ahora es real.";
            textoUI.alpha = 0f;
        }

        // Reproducir audio especial
        if (sonidoMomentoSombra != null)
        {
            audioSource.clip = sonidoMomentoSombra;
            audioSource.volume = 1f;
            audioSource.Play();
        }

        // ---- FADE IN ----
        float fadeInDur = 2f;
        float t = 0f;
        while (t < fadeInDur)
        {
            t += Time.deltaTime;
            if (textoUI != null)
                textoUI.alpha = Mathf.Lerp(0f, 1f, t / fadeInDur);
            yield return null;
        }

        // Mantener texto visible durante el audio o un mínimo
        float duracionVisible = Mathf.Max(sonidoMomentoSombra != null ? sonidoMomentoSombra.length : 5f, 5f);
        yield return new WaitForSeconds(duracionVisible - fadeInDur);

        // ---- FADE OUT ----
        float fadeOutDur = 2f;
        t = 0f;
        while (t < fadeOutDur)
        {
            t += Time.deltaTime;
            if (textoUI != null)
                textoUI.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDur);
            yield return null;
        }

        if (textoUI != null)
            textoUI.text = "";

        // Restaurar volumen
        AudioListener.volume = 1f;

        Debug.Log("[KeySpawnManager] Fin del momento poético. Continuando con la llave...");
    }

    // -------------------------------------------------------
    // SPAWN DE LA LLAVE
    // -------------------------------------------------------
    private IEnumerator SpawnLlaveDespuesDeDelay()
    {
        yield return new WaitForSeconds(delayAparicion);

        if (llavePrefab == null || puntosSpawn.Count == 0)
        {
            Debug.LogWarning("[KeySpawnManager] No se puede spawnear la llave: faltan referencias.");
            yield break;
        }

        Transform punto = puntosSpawn[Random.Range(0, puntosSpawn.Count)];
        llaveInstanciada = Instantiate(llavePrefab, punto.position, punto.rotation);

        Collider col = llaveInstanciada.GetComponent<Collider>();
        if (col == null)
            col = llaveInstanciada.AddComponent<BoxCollider>();
        col.isTrigger = true;

        Debug.Log($"[KeySpawnManager] 🔑 Llave generada en: {punto.name}\n" +
                  $"→ Posición: {punto.position}\n" +
                  $"→ Tiempo: {Time.time:F2} segundos desde el inicio.");
    }

    // -------------------------------------------------------
    // RECOGER LA LLAVE
    // -------------------------------------------------------
    private void OnTriggerStay(Collider other)
    {
        if (llaveInstanciada != null && other.CompareTag("Player"))
        {
            Collider llaveCol = llaveInstanciada.GetComponent<Collider>();
            if (llaveCol != null && other.bounds.Intersects(llaveCol.bounds))
            {
                Debug.Log("[KeySpawnManager] Jugador recogió la llave.");

                // Sonido de recogida
                if (sonidoRecogerLlave != null)
                {
                    audioSource.clip = sonidoRecogerLlave;
                    audioSource.Play();
                    Debug.Log("[KeySpawnManager] Sonido de llave reproducido.");
                }

                // Ocultar visualmente la llave
                foreach (var rend in llaveInstanciada.GetComponentsInChildren<Renderer>())
                    rend.enabled = false;

                float delay = (sonidoRecogerLlave != null) ? sonidoRecogerLlave.length : 0f;
                Destroy(llaveInstanciada, delay);
                llaveInstanciada = null;
            }
        }
    }
}
