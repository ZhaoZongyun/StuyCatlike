using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate float Function(float x, float z, float t);

    public static Function[] functions = { Wave, MultiWave, Ripple, Ripple2 };

    public static Function GetFunction(int index)
    {
        return functions[index];
    }

    public static float Wave(float x, float z, float t)
    {
        float y = Sin(PI * (x + z + t));
        return y;
    }

    public static float MultiWave(float x, float z, float t)
    {
        float y = Sin(PI * (x + t));
        y += Sin(2f * PI * (x + t)) * 0.5f;
        return y * (2f / 3f);
    }

    public static float Ripple(float x, float z, float t)
    {
        float d = Abs(x);
        return d;
    }


    public static float Ripple2(float x, float z, float t)
    {
        float d = Abs(x);
        float y = Sin(4f * PI * d);
        return y;
    }
}
