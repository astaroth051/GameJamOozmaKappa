using UnityEngine;

public class CenterPivot : MonoBehaviour
{
    [ContextMenu("Centrar pivote en malla")]
    void Center()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("No hay MeshFilter válido en este objeto.");
            return;
        }

        Mesh mesh = mf.sharedMesh;
        Vector3 center = mesh.bounds.center;

        // Mueve la geometría al centro del pivot
        transform.position += transform.TransformPoint(center) - transform.position;

        // Corrige el offset en la malla
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] -= center;
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        
        Debug.Log(" Pivote centrado correctamente.");
    }
}
