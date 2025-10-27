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

    [Header("Texto TMP en pantalla")]
    public TextMeshProUGUI textoUI; // Asigna el TextMeshProUGUI del Canvas por el inspector

    private GameObject llaveInstanciada;
    private bool llaveSpawned = false;
    private bool momentoSombraMostrado = false;

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

        // Inicializar texto
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
    // MOMENTO POÉTICO (solo texto visual)
    // -------------------------------------------------------
    private IEnumerator MostrarMomentoSombra()
    {
        Debug.Log("[KeySpawnManager] 🕯️ Momento poético: la sombra fue vista.");

        if (textoUI != null)
        {
            textoUI.text = "La sombra dejó de ocultarse... la viste a los ojos, y la llave ahora es real.";
            textoUI.alpha = 0f;
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

        // Mantener texto visible unos segundos
        yield return new WaitForSeconds(4f);

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
    // RECOGER LA LLAVE (sin sonido)
    // -------------------------------------------------------
    private void OnTriggerStay(Collider other)
    {
        if (llaveInstanciada != null && other.CompareTag("Player"))
        {
            Collider llaveCol = llaveInstanciada.GetComponent<Collider>();
            if (llaveCol != null && other.bounds.Intersects(llaveCol.bounds))
            {
                Debug.Log("[KeySpawnManager] Jugador recogió la llave.");

                // Ocultar visualmente la llave
                foreach (var rend in llaveInstanciada.GetComponentsInChildren<Renderer>())
                    rend.enabled = false;

                Destroy(llaveInstanciada);
                llaveInstanciada = null;
            }
        }
    }
}
