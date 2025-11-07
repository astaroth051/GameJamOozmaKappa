using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateSelectedObjects : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tag del objeto que activará la desactivación (por defecto: Player).")]
    public string triggerTag = "Player";

    [Tooltip("Lista de objetos que se desactivarán al tocar este objeto.")]
    public List<GameObject> objetosADesactivar;

    [Tooltip("Tiempo de espera antes de desactivar (opcional).")]
    public float delayBeforeDeactivate = 0f;

    [Tooltip("Desactivar también este objeto al final.")]
    public bool disableSelf = false;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(triggerTag)) return;

        triggered = true;
        StartCoroutine(DesactivarObjetos());
    }

    private IEnumerator DesactivarObjetos()
    {
        if (delayBeforeDeactivate > 0f)
            yield return new WaitForSeconds(delayBeforeDeactivate);

        foreach (GameObject obj in objetosADesactivar)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log("[DeactivateSelectedObjects] Desactivado: " + obj.name);
            }
        }

        if (disableSelf)
            gameObject.SetActive(false);

        Debug.Log("[DeactivateSelectedObjects] Todos los objetos seleccionados fueron desactivados.");
    }
}
