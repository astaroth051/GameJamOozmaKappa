using UnityEngine;

public class CombineTreeMeshes : MonoBehaviour
{
    [ContextMenu("Combinar árbol")]
    void Combine()
    {
        // Obtiene todas las mallas de los hijos
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Material[] materials = new Material[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            if (meshFilters[i].sharedMesh == null)
            {
                i++;
                continue;
            }

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

            MeshRenderer renderer = meshFilters[i].GetComponent<MeshRenderer>();
            if (renderer != null)
                materials[i] = renderer.sharedMaterial;

            i++;
        }

        // Crear nueva malla combinada
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine, false); // false = conserva materiales separados

        // Añadir MeshFilter y Renderer al padre
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        mf.sharedMesh = combinedMesh;
        mr.sharedMaterials = materials;

        Debug.Log(" Árbol combinado correctamente en " + gameObject.name);
    }
}
