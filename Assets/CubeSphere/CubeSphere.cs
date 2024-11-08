using TMPro;
using UnityEngine;

/// <summary>
/// 生成一个球
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour
{
    public GameObject prefab;
    public int gridSize;
    public int radius;

    private void Awake()
    {
        Generate();
    }

    private Vector3[] vertices;
    private Vector2[] uv;
    private Vector3[] normals;
    private Color32[] colors;
    private int[] triangles;
    private Mesh mesh;
    private int ring;

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        // 顶点
        int cornerCount = 8; //8个角
        int edgeCount = (gridSize - 2 + gridSize - 2 + gridSize - 2) * 4; // 12个边
        int faceCount = ((gridSize - 2) * (gridSize - 2) + (gridSize - 2) * (gridSize - 2) + (gridSize - 2) * (gridSize - 2)) * 2; //4个面
        ring = (gridSize + gridSize) * 2 - 4;

        vertices = new Vector3[cornerCount + edgeCount + faceCount];
        uv = new Vector2[vertices.Length];
        normals = new Vector3[vertices.Length];
        colors = new Color32[vertices.Length];

        CreateVertices();
        mesh.vertices = vertices;

        // 创建三角面
        //CreateTriangles();
        //mesh.triangles = triangles;

        // 创建多个三角面组（子网格）
        CreateSubTriangles();

        mesh.uv = uv;
        mesh.normals = normals;
        mesh.colors32 = colors;
    }

    private void CreateVertices()
    {
        int index = 0;
        for (int y = 0; y < gridSize; y++)
        {
            // 生成一层
            for (int x = 0; x < gridSize; x++)
                SetVertex(index++, x, y, 0);

            for (int z = 1; z < gridSize; z++)
                SetVertex(index++, gridSize - 1, y, z);

            for (int x = gridSize - 2; x >= 0; x--)
                SetVertex(index++, x, y, gridSize - 1);

            for (int z = gridSize - 2; z > 0; z--)
                SetVertex(index++, 0, y, z);
        }

        // 顶部
        for (int z = 1; z < gridSize - 1; z++)
        {
            for (int x = 1; x < gridSize - 1; x++)
            {
                SetVertex(index++, x, gridSize - 1, z);
            }
        }

        // 底部
        for (int z = 1; z < gridSize - 1; z++)
        {
            for (int x = 1; x < gridSize - 1; x++)
            {
                SetVertex(index++, x, 0, z);
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            var go = Instantiate(prefab);
            var text = go.transform.GetChild(0).GetComponent<TextMeshPro>();
            go.transform.position = vertices[i];
            text.text = i.ToString();
        }

        CreateTriangles();
    }

    private void CreateTriangles()
    {
        // 三角面
        int quads = ((gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1)) * 2;
        triangles = new int[quads * 6]; //一个四角面，6个顶点
        int t = 0, v = 0;   //t 为 triangles 索引，v 为 顶点索引

        // 立方体中部四个面
        for (int y = 0; y < gridSize - 1; y++, v++)
        {
            // 从下到上一圈
            for (int x = 0; x < ring - 1; x++, v++)
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            t = SetQuad(triangles, t, v, v - (ring - 1), v + ring, v + 1);
        }

        t = SetTopFace(triangles, t);
        SetBottomFace(triangles, t);
    }

    // 创建子网格，前后左右上下，分别为 trianglesZ、trianglesX、trianglesY
    private void CreateSubTriangles()
    {
        int[] trianglesZ = new int[gridSize * gridSize * 6 * 2];
        int[] trianglesX = new int[gridSize * gridSize * 6 * 2];
        int[] trianglesY = new int[gridSize * gridSize * 6 * 2];
        int v = 0;
        int tZ = 0;
        int tX = 0;
        int tY = 0;
        for (int y = 0; y < gridSize - 1; y++, v++)
        {
            for (int x = 0; x < gridSize - 1; x++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int z = 0; z < gridSize - 1; z++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            for (int x = 0; x < gridSize - 1; x++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int z = 0; z < gridSize - 2; z++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }

        tY = SetTopFace(trianglesY, tY);
        SetBottomFace(trianglesY, tY);

        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);
    }

    private void SetVertex(int i, float x, float y, float z)
    {
        // cube 中心点放到偏移到原点
        Vector3 v = new Vector3(x, y, z) / (gridSize - 1) - Vector3.one * 0.5f;
        // 网格所有顶点都设到球面上
        normals[i] = v.normalized;
        vertices[i] = normals[i] * radius;
        colors[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    int start = 0, right = 0, leftTop = 0, rightTop, leftBorder = 0, rightBorder = 0;
    private int SetTopFace(int[] triangles, int t)
    {
        // 顶部
        start = ring * (gridSize - 1);
        right = start + 1;
        leftTop = start + ring - 1;
        rightTop = start + ring;

        // 顶部第一行
        for (int x = 0; x < gridSize - 2; x++, start++, right++, leftTop++, rightTop++)
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        rightTop = right + 1;
        t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        // 顶部第二行~倒数第二行
        leftBorder = ring * gridSize - 1;
        rightBorder = rightTop;
        for (int z = 0; z < gridSize - 3; z++, leftBorder--, rightBorder++)
        {
            // 第一个四角面
            start = leftBorder;
            right = leftBorder + 1 + (gridSize - 1) * z;
            leftTop = leftBorder - 1;
            rightTop = right + (gridSize - 2);
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);

            // 中间的四角面
            start = right;
            right = right + 1;
            leftTop = rightTop;
            rightTop = leftTop + 1;
            for (int x = 0; x < gridSize - 3; x++, start++, right++, leftTop++, rightTop++)
                t = SetQuad(triangles, t, start, right, leftTop, rightTop);

            // 最后一个四角面
            right = rightBorder;
            rightTop = rightBorder + 1;
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);
        }

        // 顶部倒数第一行
        // 第一个四角面
        start = leftBorder;
        right = vertices.Length - (gridSize - 2) * (gridSize - 2) - (gridSize - 2);
        leftTop = leftBorder - 1;
        rightTop = leftTop - 1;
        t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        // 中间的四角面
        start = right;
        right = start + 1;
        leftTop = rightTop;
        rightTop = leftTop - 1;
        for (int x = 0; x < gridSize - 3; x++, start++, right++, leftTop--, rightTop--)
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        // 最后一个四角面
        right = rightBorder;
        rightTop = rightBorder + 1;
        t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        return t;
    }

    private int SetBottomFace(int[] triangles, int t)
    {
        // 底部
        start = 0;
        leftTop = ring - 1;
        right = start + 1;
        rightTop = vertices.Length - ((gridSize - 2) * (gridSize - 2));

        // 底部第一行
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        start = 1;
        leftTop = rightTop;
        right = start + 1;
        rightTop = leftTop + 1;

        // 底部第一行，中间的四角面
        for (int x = 0; x < gridSize - 3; x++, start++, leftTop++, right = start + 1, rightTop++)
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        // 底部第一行最后一个四角面
        rightTop = start + 2;
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        // 底部第二行 ~ 倒数第二行
        leftBorder = ring - 1;
        rightBorder = gridSize;
        for (int z = 0; z < gridSize - 3; z++, leftBorder--, rightBorder++)
        {
            start = leftBorder;
            leftTop = leftBorder - 1;
            right = vertices.Length - ((gridSize - 2) * (gridSize - 2)) + (gridSize - 2) * z;
            rightTop = right + (gridSize - 2);

            // 第一个四角面
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);

            start = right;
            leftTop = rightTop;
            right = start + 1;
            rightTop = leftTop + 1;

            // 中间的四角面
            for (int x = 0; x < gridSize - 3; x++, start++, leftTop++, right++, rightTop++)
                t = SetQuad(triangles, t, start, leftTop, right, rightTop);

            // 最后一个四角面
            right = rightBorder;
            rightTop = right + 1;
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);
        }

        // 底部倒数第一行
        start = leftBorder;
        leftTop = leftBorder - 1;
        right = vertices.Length - (gridSize - 2);
        rightTop = leftTop - 1;
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        start = right;
        leftTop = rightTop;
        right = start + 1;
        rightTop = leftTop - 1;

        // 底部倒数第一行，中间的四角面
        for (int x = 0; x < gridSize - 3; x++, start++, leftTop--, right++, rightTop--)
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        // 底部倒数第一行，最后一个四角面
        right = rightBorder;
        rightTop = rightBorder + 1;
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        return t;
    }

    private int SetQuad(int[] triangles, int i, int start, int right, int leftTop, int rightTop)
    {
        triangles[i] = start;
        triangles[i + 1] = triangles[i + 4] = leftTop;
        triangles[i + 2] = triangles[i + 3] = right;
        triangles[i + 5] = rightTop;
        return i + 6;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }
}
