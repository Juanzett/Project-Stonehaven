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

        var s = settings.seed != 0 ? settings.seed : seed;
        var rng = new System.Random(s);

        // 1) Macro biomas
        var biomeMap = BuildBiomeMap(settings.mapWidth, settings.mapHeight, settings.biomes, s);

        // 2) Máscara de agua coherente por bioma (agua solo donde tiene sentido)
        var waterMask = BuildWaterMask(settings.mapWidth, settings.mapHeight, biomeMap, settings.biomes, s);

        // 3) Render por chunks usando WFC con restricciones por celda
        tilemap.ClearAllTiles();
        int w = settings.mapWidth;
        int h = settings.mapHeight;
        int cs = Mathf.Max(8, settings.chunkSize);

        for (int cy = 0; cy < h; cy += cs)
        for (int cx = 0; cx < w; cx += cs)
        {
            int cw = Mathf.Min(cs, w - cx);
            int ch = Mathf.Min(cs, h - cy);

            // Construir dominios permitidos por celda según la máscara global de agua
            var allowed = BuildAllowedIdsForChunk(cx, cy, cw, ch, waterMask, w, h);

            var tiles = WFCGenerator.Generate(terrainTileset, cw, ch, rng, allowed);

            for (int y = 0; y < ch; y++)
            for (int x = 0; x < cw; x++)
            {
                var tile = tiles[x, y];
                if (tile != null)
                    tilemap.SetTile(new Vector3Int(cx + x, cy + y, 0), tile);
            }
        }

        Debug.Log($"World generated (coherent). Seed={s}, Size={settings.mapWidth}x{settings.mapHeight}, Chunk={settings.chunkSize}");
    }

    private int[,] BuildBiomeMap(int width, int height, List<BiomeConfig> biomes, int seed)
    {
        var map = new int[width, height];
        if (biomes == null || biomes.Count == 0) return map;

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
                if (score > bestScore) { bestScore = score; best = i; }
            }
            map[x, y] = best;
        }
        return map;
    }

    // Agua = bioma de agua o lago permitido por ruido local. Resultado: true = agua
    private bool[,] BuildWaterMask(int width, int height, int[,] biomeMap, List<BiomeConfig> biomes, int seed)
    {
        var mask = new bool[width, height];
        float ox = (seed * 1.123f) % 1000f;
        float oy = (seed * 2.357f) % 1000f;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var biome = biomes != null && biomes.Count > 0 ? biomes[biomeMap[x, y]] : null;
            bool isWater = biome != null && biome.isWaterBiome;

            if (!isWater && biome != null && biome.allowLakes)
            {
                // Lagos locales por ruido, opcionalmente modulados por falloff para evitar bordes
                float n = Mathf.PerlinNoise(ox + x * biome.lakeNoiseScale, oy + y * biome.lakeNoiseScale);
                float fall = 1f - NoiseUtils.Falloff01(x, y, width, height); // más tierra en el centro
                float v = Mathf.Clamp01(n * 0.8f + fall * 0.2f);
                if (v > biome.lakeThreshold) isWater = true;
            }

            mask[x, y] = isWater;
        }
        return mask;
    }

    // Construye por celda la lista de IDs permitidos (dominio) en el chunk según agua/tierra y orientación de costa.
    private List<string>[,] BuildAllowedIdsForChunk(int cx, int cy, int cw, int ch, bool[,] waterMask, int width, int height)
    {
        var allowed = new List<string>[cw, ch];

        for (int y = 0; y < ch; y++)
        for (int x = 0; x < cw; x++)
        {
            int gx = cx + x;
            int gy = cy + y;
            bool water = waterMask[gx, gy];

            // Vecinos (tratamos fuera de límites como tierra para no forzar costa al borde del mapa)
            bool n = gy + 1 < height ? waterMask[gx, gy + 1] : false;
            bool e = gx + 1 < width  ? waterMask[gx + 1, gy] : false;
            bool s = gy - 1 >= 0     ? waterMask[gx, gy - 1] : false;
            bool w = gx - 1 >= 0     ? waterMask[gx - 1, gy] : false;

            var list = new List<string>(4);

            if (water)
            {
                // Interior de agua: solo agua
                list.Add("water_center");
            }
            else
            {
                int count = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);
                if (count == 0)
                {
                    list.Add("grass_center");
                }
                else if (count == 1)
                {
                    if (n) list.Add("grass_edge_n");
                    else if (e) list.Add("grass_edge_e");
                    else if (s) list.Add("grass_edge_s");
                    else if (w) list.Add("grass_edge_w");
                }
                else if (count == 2)
                {
                    // Esquina si son perpendiculares
                    if (n && e) list.Add("grass_corner_ne");
                    else if (n && w) list.Add("grass_corner_nw");
                    else if (s && e) list.Add("grass_corner_se");
                    else if (s && w) list.Add("grass_corner_sw");
                    else
                    {
                        // Opuestos (n&s) o (e&w): permitir centro o ambos edges para que WFC resuelva
                        list.Add("grass_center");
                        if (n && s) { list.Add("grass_edge_n"); list.Add("grass_edge_s"); }
                        if (e && w) { list.Add("grass_edge_e"); list.Add("grass_edge_w"); }
                    }
                }
                else if (count == 3)
                {
                    // Casi rodeado de agua: usar el borde hacia el lado seco para cerrar bien
                    if (!n) list.Add("grass_edge_s");
                    else if (!e) list.Add("grass_edge_w");
                    else if (!s) list.Add("grass_edge_n");
                    else if (!w) list.Add("grass_edge_e");
                }
                else // count == 4 (islote de tierra 1x1 dentro de agua)
                {
                    list.Add("grass_center");
                }
            }

            // Seguridad: si algo falló, al menos un centro
            if (list.Count == 0) list.Add(water ? "water_center" : "grass_center");
            allowed[x, y] = list;
        }

        return allowed;
    }
}