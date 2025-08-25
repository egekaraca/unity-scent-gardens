using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CircleMeshGenerator : MonoBehaviour
{
    [Range(3, 512)]
    public int segments = 128;
    public float radius = 5f;
    public bool orientAlongY = true;

    private void OnValidate()
    {
        GenerateMesh();
    }

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralCircle";

        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];
        Vector2[] uvs = new Vector2[vertices.Length];

        // Merkez vertex
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        // Kenar vertexleri (tam çember)
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            // Eksen yönü
            if (orientAlongY)
                vertices[i + 1] = new Vector3(x, 0, y);
            else
                vertices[i + 1] = new Vector3(x, y, 0);

            uvs[i + 1] = new Vector2((x / (radius * 2f)) + 0.5f, (y / (radius * 2f)) + 0.5f);
        }

        // Üçgenler
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
