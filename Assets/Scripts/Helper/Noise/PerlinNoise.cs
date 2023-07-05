using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : Noise
{
    private float Scale;
    private int OffsetX;
    private int OffsetY;

    public PerlinNoise(float scale = 0.2f)
    {
        Scale = scale;
        OffsetX = Random.Range(-100000, 100000);
        OffsetY = Random.Range(-100000, 100000);
    }

    public override float GetValue(float x, float y)
    {
        return Mathf.PerlinNoise(OffsetX + x * Scale, OffsetY + y * Scale);
    }
}
