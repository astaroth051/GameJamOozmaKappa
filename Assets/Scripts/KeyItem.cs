using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyItem : MonoBehaviour
{
    [Header("Audio al recoger la llave")]
    public AudioClip sonidoRecogerLlave;

    public static bool llaveRecogida = false;

    private bool recogida = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogida) return;

        if (other.CompareTag("Player"))
        {
            recogida = true;
            llaveRecogida = true;

            Debug.Log("[KeyItem] Llave recogida por el jugador.");

            if (sonidoRecogerLlave != null)
            {
                // Crear un AudioSource temporal que suene encima de todo
                GameObject tempAudio = new GameObject("TempKeySound");
                AudioSource s = tempAudio.AddComponent<AudioSource>();
                s.clip = sonidoRecogerLlave;
                s.spatialBlend = 0f; // 2D
                s.volume = 1f;
                s.priority = 0; // máxima prioridad para no cortarse
                s.Play();
                Destroy(tempAudio, sonidoRecogerLlave.length + 0.2f);

                Debug.Log("[KeyItem] Sonido de recogida reproducido (fuente temporal).");
            }

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            float delay = (sonidoRecogerLlave != null) ? sonidoRecogerLlave.length : 0f;
            Destroy(gameObject, delay);
        }
    }
}
