using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Config")]
    public GenerationSettings settings;
    public WFCTileset terrainTileset;

    [Header("Output")]
    public Tilemap tilemap; // Requiere Grid > Tilemap en la escena.

    public void Generate(int seed)
    {
        if (settings == null) { Debug.LogError("GenerationSettings no asignado"); return; }
        if (tilemap == null) { Debug.LogError("Tilemap no asignado"); return; }

        // Semilla
        var s = settings.seed != 0 ? settings.seed : seed;
        var rng = new System.Random(s);

        // Macro: elegir bioma por Perlin (muy simple: tomar el de mayor ruido ponderado).
        var biomeMap = BuildBiomeMap(settings.mapWidth, settings.mapHeight, settings.biomes, s);

        // Render por chunks: por ahora un solo tileset (del terreno). WFC para micro-patrones.
        tilemap.ClearAllTiles();
        int w = settings.mapWidth;
        int h = settings.mapHeight;
        int cs = Mathf.Max(8, settings.chunkSize);

        for (int cy = 0; cy < h; cy += cs)
        for (int cx = 0; cx < w; cx += cs)
        {
            int cw = Mathf.Min(cs, w - cx);
            int ch = Mathf.Min(cs, h - cy);

            // En el futuro: escoger tileset por bioma dominante del chunk.
            var tiles = WFCGenerator.Generate(terrainTileset, cw, ch, rng);

            // Pintar
            for (int y = 0; y < ch; y++)
            for (int x = 0; x < cw; x++)
            {
                var tile = tiles[x, y];
                if (tile != null)
                    tilemap.SetTile(new Vector3Int(cx + x, cy + y, 0), tile);
            }
        }

        Debug.Log($"World generated. Seed={s}, Size={settings.mapWidth}x{settings.mapHeight}, Chunk={settings.chunkSize}");
    }

    private int[,] BuildBiomeMap(int width, int height, List<BiomeConfig> biomes, int seed)
    {
        var map = new int[width, height];
        if (biomes == null || biomes.Count == 0)
        {
            // Fallback: todo 0
            return map;
        }

        // Generar perlin por bioma y elegir el índice con mayor score (noise * weight)
        float ox = Mathf.Abs(seed % 10000) / 1000f;
        float oy = Mathf.Abs(seed % 20000) / 1000f;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int best = 0;
            float bestScore = float.MinValue;

            for (int i = 0; i < biomes.Count; i++)
            {
                var b = biomes[i];
                float n = Mathf.PerlinNoise(ox + x * b.perlinScale, oy + y * b.perlinScale);
                float score = (n - (1f - b.threshold)) * b.weight;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }
            map[x, y] = best;
        }
        return map;
    }
}