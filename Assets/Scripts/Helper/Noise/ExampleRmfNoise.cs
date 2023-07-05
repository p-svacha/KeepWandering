using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleRmfNoise : Noise
{
    private RidgedMultifractalNoise RmfNoise;
    private float Scale;

    public ExampleRmfNoise(float scale = 0.1f)
    {
        Scale = scale;
        RmfNoise = new RidgedMultifractalNoise(1, 2, 6, Random.Range(int.MinValue, int.MaxValue));
    }

    public override float GetValue(float x, float y)
    {
        float val = (float)(RmfNoise.GetValue(x * Scale, y * Scale, 1));
        return val;
    }
}
