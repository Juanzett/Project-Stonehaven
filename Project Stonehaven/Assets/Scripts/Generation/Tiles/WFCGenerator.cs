using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WFCGenerator
{
    // Overload con restricciones por celda (lista de IDs permitidos).
    public static TileBase[,] Generate(WFCTileset tileset, int width, int height, System.Random rng, List<string>[,] allowedPerCell)
    {
        if (tileset == null || tileset.modules.Count == 0)
            throw new ArgumentException("WFCTileset vacío");

        var result = new TileBase[width, height];

        // Indexar módulos por ID para filtrar rápido.
        var byId = new Dictionary<string, WFCTileset.TileModule>(StringComparer.Ordinal);
        foreach (var m in tileset.modules)
            if (!byId.ContainsKey(m.id)) byId[m.id] = m;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            // Candidatos iniciales: restringidos por allowedPerCell si existe.
            var candidates = new List<WFCTileset.TileModule>();
            var allowed = allowedPerCell != null ? allowedPerCell[x, y] : null;
            if (allowed != null && allowed.Count > 0)
            {
                foreach (var id in allowed)
                    if (byId.TryGetValue(id, out var mod)) candidates.Add(mod);
                if (candidates.Count == 0)
                    candidates.AddRange(tileset.modules); // fallback si las IDs no coinciden
            }
            else
            {
                candidates.AddRange(tileset.modules);
            }

            // Filtrar por vecinos ya colapsados (N y W, dado el recorrido)
            if (y - 1 >= 0) FilterByNeighbor(candidates, tileset, result[x, y - 1], dir: 0); // norte
            if (x - 1 >= 0) FilterByNeighbor(candidates, tileset, result[x - 1, y], dir: 3); // oeste

            var chosen = ChooseWeighted(candidates, rng);
            result[x, y] = chosen?.tile ?? tileset.modules[0].tile;
        }

        return result;
    }

    // Compatibilidad con llamadas existentes.
    public static TileBase[,] Generate(WFCTileset tileset, int width, int height, System.Random rng)
        => Generate(tileset, width, height, rng, null);

    private static void FilterByNeighbor(List<WFCTileset.TileModule> candidates, WFCTileset tileset, TileBase neighborTile, int dir)
    {
        if (neighborTile == null) return;
        WFCTileset.TileModule neighbor = null;
        foreach (var m in tileset.modules)
        {
            if (m.tile == neighborTile) { neighbor = m; break; }
        }
        if (neighbor == null) return;

        List<string> allowedIdsFromNeighbor = null;
        switch (dir)
        {
            case 0: allowedIdsFromNeighbor = neighbor.south; break; // vecino al norte: miramos su sur
            case 3: allowedIdsFromNeighbor = neighbor.east;  break; // vecino al oeste: miramos su este
        }
        if (allowedIdsFromNeighbor == null || allowedIdsFromNeighbor.Count == 0) return;

        candidates.RemoveAll(c => !allowedIdsFromNeighbor.Contains(c.id));
    }

    private static WFCTileset.TileModule ChooseWeighted(List<WFCTileset.TileModule> candidates, System.Random rng)
    {
        if (candidates.Count == 0) return null;
        float total = 0f;
        foreach (var c in candidates) total += c.weight;
        float r = (float)(rng.NextDouble() * total);
        foreach (var c in candidates)
        {
            if (r < c.weight) return c;
            r -= c.weight;
        }
        return candidates[candidates.Count - 1];
    }
}