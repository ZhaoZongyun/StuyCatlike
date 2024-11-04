using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://catlikecoding.com/unity/tutorials/procedural-grid/

/// <summary>
/// 生成一个面的网格
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestGrid : MonoBehaviour
{
    public int xSize, ySize;

    private void Awake()
    {
        Generate();
    }

    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;
    private Vector4[] tangents;
    private Mesh mesh;

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        //vertices UV
        vertices = new Vector3[xSize * ySize];
        uv = new Vector2[xSize * ySize];
        tangents = new Vector4[xSize * ySize];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int index = 0, j = 0; j < ySize; j++)
        {
            for (int i = 0; i < xSize; i++, index++)
            {
                vertices[index] = new Vector3(i, j);
                uv[index] = new Vector2((float)i / (xSize - 1), (float)j / (ySize - 1));
                tangents[index] = tangent;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        triangles = new int[(xSize - 1) * (ySize - 1) * 6];

        for (int j = 0, index = 0; j < ySize - 1; j++)
        {
            for (int i = 0; i < xSize - 1; i++, index += 6)
            {
                triangles[index] = i + ySize * j;
                triangles[index + 1] = triangles[index + 4] = i + ySize * (j + 1);
                triangles[index + 2] = triangles[index + 3] = i + ySize * j + 1;
                triangles[index + 5] = i + ySize * (j + 1) + 1;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null) return;

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
