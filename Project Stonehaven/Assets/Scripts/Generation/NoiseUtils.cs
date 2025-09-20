using UnityEngine;

public static class NoiseUtils
{
    // FBM simple
    public static float FBM(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
    {
        float sum = 0f;
        float amp = 1f;
        float freq = 1f;
        float norm = 0f;

        for (int i = 0; i < octaves; i++)
        {
            sum += Mathf.PerlinNoise(x * freq, y * freq) * amp;
            norm += amp;
            amp *= gain;
            freq *= lacunarity;
        }
        return Mathf.Clamp01(sum / Mathf.Max(0.0001f, norm));
    }

    // Falloff tipo "isla/continente": 0 centro, 1 bordes (sirve para restar)
    public static float Falloff01(int x, int y, int width, int height, float a = 3f, float b = 2.2f)
    {
        float nx = x / (float)width * 2f - 1f;
        float ny = y / (float)height * 2f - 1f;
        float v = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));
        float fall = Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - b * v, a));
        return Mathf.Clamp01(fall);
    }
}