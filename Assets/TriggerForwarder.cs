using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerForwarder : MonoBehaviour
{
    [Tooltip("Referencia al controlador principal de la secuencia funeraria")]
    public FuneralSequenceController funeralController;

    [Tooltip("Marca si este trigger es el primero (true) o el segundo (false)")]
    public bool isFirstZone = true;

    private void Awake()
    {
        Collider c = GetComponent<Collider>();
        if (!c.isTrigger)
        {
            Debug.LogWarning("[Forwarder] El collider de " + name + " no está marcado como Trigger. Se forzará como Trigger.");
            c.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Forwarder] Player entró en " + name + " | isFirstZone = " + isFirstZone);

            if (funeralController != null)
            {
                funeralController.OnZoneEntered(isFirstZone);
            }
            else
            {
                Debug.LogWarning("[Forwarder] No hay referencia al FuneralSequenceController en " + name);
            }
        }
    }
}
