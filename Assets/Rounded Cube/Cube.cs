using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// https://catlikecoding.com/unity/tutorials/rounded-cube/

/// <summary>
/// 生成一个立方体网格
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public GameObject prefab;
    public int xSize, ySize, zSize;

    private void Awake()
    {
        Generate();
    }

    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;
    private Vector4[] tangents;
    private Mesh mesh;
    private int ring;

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        // 顶点
        int cornerVertices = 8; //8个角
        int edgeVertices = (xSize + ySize + zSize - 6) * 4; // 12个边
        int faceVertices = ((xSize - 2) * (ySize - 2) + (xSize - 2) * (zSize - 2) + (ySize - 2) * (zSize - 2)) * 2; //4个面

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        uv = new Vector2[vertices.Length];

        int index = 0;

        for (int y = 0; y < ySize; y++)
        {
            // 生成一层
            for (int x = 0; x < xSize - 1; x++)
                vertices[index++] = new Vector3(x, y, 0);

            for (int z = 0; z < zSize - 1; z++)
                vertices[index++] = new Vector3(xSize - 1, y, z);

            for (int x = xSize - 1; x > 0; x--)
                vertices[index++] = new Vector3(x, y, zSize - 1);

            for (int z = zSize - 1; z > 0; z--)
                vertices[index++] = new Vector3(0, y, z);
        }

        // 顶部
        for (int z = 1; z < zSize - 1; z++)
        {
            for (int x = 1; x < xSize - 1; x++)
            {
                vertices[index++] = new Vector3(x, ySize - 1, z);
            }
        }

        // 底部
        for (int z = 1; z < zSize - 1; z++)
        {
            for (int x = 1; x < xSize - 1; x++)
            {
                vertices[index++] = new Vector3(x, 0, z);
            }
        }

        mesh.vertices = vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            var go = Instantiate(prefab) as GameObject;
            var text = go.transform.GetChild(0).GetComponent<TextMeshPro>();
            go.transform.position = vertices[i];
            text.text = i.ToString();
        }

        // 三角面
        int quads = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;
        triangles = new int[quads * 6]; //一个四角面，6个顶点
        ring = (xSize + zSize) * 2 - 4;
        int t = 0, v = 0;   //t 为 triangles 索引，v 为 顶点索引

        // 立方体中部四个面
        for (int y = 0; y < ySize - 1; y++, v++)
        {
            // 从下到上一圈
            for (int x = 0; x < ring - 1; x++, v++)
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            t = SetQuad(triangles, t, v, v - (ring - 1), v + ring, v + 1);
        }

        t = SetTopFace(t);
        SetBottomFace(t);

        mesh.triangles = triangles;
    }

    int start = 0, right = 0, leftTop = 0, rightTop, leftBorder = 0, rightBorder = 0;
    private int SetTopFace(int t)
    {
        // 顶部
        start = ring * (ySize - 1);
        right = start + 1;
        leftTop = start + ring - 1;
        rightTop = start + ring;

        // 顶部第一行
        for (int x = 0; x < xSize - 2; x++, start++, right++, leftTop++, rightTop++)
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        rightTop = right + 1;
        t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        // 顶部第二行~倒数第二行
        leftBorder = ring * ySize - 1;
        rightBorder = rightTop;
        for (int z = 0; z < zSize - 3; z++, leftBorder--, rightBorder++)
        {
            // 第一个四角面
            start = leftBorder;
            right = leftBorder + 1 + (xSize - 1) * z;
            leftTop = leftBorder - 1;
            rightTop = right + (xSize - 2);
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);

            // 中间的四角面
            start = right;
            right = right + 1;
            leftTop = rightTop;
            rightTop = leftTop + 1;
            for (int x = 0; x < xSize - 3; x++, start++, right++, leftTop++, rightTop++)
                t = SetQuad(triangles, t, start, right, leftTop, rightTop);

            // 最后一个四角面
            right = rightBorder;
            rightTop = rightBorder + 1;
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);
        }

        // 顶部倒数第一行
        // 第一个四角面
        start = leftBorder;
        right = vertices.Length - (xSize - 2) * (zSize - 2) - (xSize - 2);
        leftTop = leftBorder - 1;
        rightTop = leftTop - 1;
        t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        // 中间的四角面
        start = right;
        right = start + 1;
        leftTop = rightTop;
        rightTop = leftTop - 1;
        for (int x = 0; x < xSize - 3; x++, start++, right++, leftTop--, rightTop--)
            t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        // 最后一个四角面
        right = rightBorder;
        rightTop = rightBorder + 1;
        t = SetQuad(triangles, t, start, right, leftTop, rightTop);

        return t;
    }

    private int SetBottomFace(int t)
    {
        // 底部
        start = 0;
        leftTop = ring - 1;
        right = start + 1;
        rightTop = vertices.Length - ((xSize - 2) * (zSize - 2));

        // 底部第一行
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        start = 1;
        leftTop = rightTop;
        right = start + 1;
        rightTop = leftTop + 1;

        // 底部第一行，中间的四角面
        for (int x = 0; x < xSize - 3; x++, start++, leftTop++, right = start + 1, rightTop++)
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        // 底部第一行最后一个四角面
        rightTop = start + 2;
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        // 底部第二行 ~ 倒数第二行
        leftBorder = ring - 1;
        rightBorder = xSize;
        for (int z = 0; z < zSize - 3; z++, leftBorder--, rightBorder++)
        {
            start = leftBorder;
            leftTop = leftBorder - 1;
            right = vertices.Length - ((xSize - 2) * (zSize - 2)) + (xSize - 2) * z;
            rightTop = right + (xSize - 2);

            // 第一个四角面
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);

            start = right;
            leftTop = rightTop;
            right = start + 1;
            rightTop = leftTop + 1;

            // 中间的四角面
            for (int x = 0; x < xSize - 3; x++, start++, leftTop++, right++, rightTop++)
                t = SetQuad(triangles, t, start, leftTop, right, rightTop);

            // 最后一个四角面
            right = rightBorder;
            rightTop = right + 1;
            t = SetQuad(triangles, t, start, leftTop, right, rightTop);
        }

        // 底部倒数第一行
        start = leftBorder;
        leftTop = leftBorder - 1;
        right = vertices.Length - (xSize - 2);
        rightTop = leftTop - 1;
        t = SetQuad(triangles, t, start, leftTop, right, rightTop);

        start = right;
        leftTop = rightTop;
        right = start + 1;
        rightTop = leftTop - 1;

        // 底部倒数第一行，中间的四角面
        for (int x = 0; x < xSize - 3; x++, start++, leftTop--, right++, rightTop--)
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
}
