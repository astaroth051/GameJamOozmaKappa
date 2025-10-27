using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyItem : MonoBehaviour
{
    [Header("Audio al recoger la llave")]
    public AudioClip sonidoRecogerLlave;

    public static bool llaveRecogida = false; // ← NUEVA VARIABLE GLOBAL

    private AudioSource audioSource;
    private bool recogida = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogida) return;

        if (other.CompareTag("Player"))
        {
            recogida = true;
            llaveRecogida = true; // ← SE ACTIVA AL RECOGER LA LLAVE

            Debug.Log("[KeyItem]  Llave recogida por el jugador.");

            if (sonidoRecogerLlave != null)
            {
                audioSource.clip = sonidoRecogerLlave;
                audioSource.Play();
                Debug.Log("[KeyItem] Sonido de recogida reproducido.");
            }

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            float delay = (sonidoRecogerLlave != null) ? sonidoRecogerLlave.length : 0f;
            Destroy(gameObject, delay);
        }
    }
}
