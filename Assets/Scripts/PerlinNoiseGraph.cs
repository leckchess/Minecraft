using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseGraph : MonoBehaviour
{
    float t = 0;
    float inc = 0.01f;

    float Map(float min, float max, float omin, float omax, float value)
    {
        return Mathf.Lerp(min, max, Mathf.InverseLerp(omin, omax, value));
    }

    void Update()
    {
        t += inc;
        float n = Mathf.PerlinNoise(t, 1);
        n = FBM(t, 3, 0.8f);
        Grapher.Log(n, "perlin avg", Color.green);
    }

    // fractal Brownian motion
    float FBM(float t, int octaves, float persistence)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for(int i=0;i<octaves;i++)
        {
            total += Mathf.PerlinNoise(t * frequency, 1) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }
}
