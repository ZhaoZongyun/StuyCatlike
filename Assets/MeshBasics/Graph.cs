using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;
    [SerializeField, Range(10, 100)]
    int resolution = 10;
    [SerializeField]
    Transform[] points;
    [SerializeField]
    int functionIndex;

    private float size;

    private void Awake()
    {
        size = 2f / resolution;
        Vector3 sacle = Vector3.one * size;
        Vector3 pos = Vector3.zero;
        points = new Transform[resolution * resolution];

        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
            }

            Transform point = points[i] = Instantiate(pointPrefab);
            float half;
            if (resolution % 2 == 0)
            {
                half = resolution * 0.5f;
                pos.x = size * (x - half) + size * 0.5f;
                pos.z = size * z;
            }
            else
            {
                half = (resolution - 1) * 0.5f;
                pos.x = size * (x - half);
                pos.z = size * z;
            }

            pos.y = pos.x * pos.x * pos.x;

            point.localPosition = pos;
            point.localScale = sacle;
            point.SetParent(transform, false);
        }
    }

    private void Update()
    {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(functionIndex);
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            position.y = f(position.x, position.z, Time.time);
            point.localPosition = position;
        }
    }
}
