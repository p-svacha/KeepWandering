using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Noise
{
    public abstract float GetValue(float x, float y);
    public float GetValue(Vector2 v) { return GetValue(v.x, v.y); }
    public float GetValue(Vector2Int v) { return GetValue(v.x, v.y); }

    public static void MakeSeamlessHorizontally(float[,] noiseMap, int stitchWidth)
    {
        int width = noiseMap.GetUpperBound(0) + 1;
        int height = noiseMap.GetUpperBound(1) + 1;

        // iterate on the stitch band (on the left
        // of the noise)
        for (int x = 0; x < stitchWidth; x++)
        {
            // get the transparency value from
            // a linear gradient
            float v = x / (float)stitchWidth;
            for (int y = 0; y < height; y++)
            {
                // compute the "mirrored x position":
                // the far left is copied on the right
                // and the far right on the left
                int o = width - stitchWidth + x;
                // copy the value on the right of the noise
                noiseMap[o, y] = Mathf.Lerp(noiseMap[o, y], noiseMap[stitchWidth - x, y], v);
            }
        }
    }
}
