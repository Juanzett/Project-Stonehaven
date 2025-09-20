using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WFCGenerator
{
    // WFC simple: recorre la grilla; para cada celda filtra por compatibilidad con vecinos ya asignados.
    public static TileBase[,] Generate(WFCTileset tileset, int width, int height, System.Random rng)
    {
        if (tileset == null || tileset.modules.Count == 0)
            throw new ArgumentException("WFCTileset vacío");

        var result = new TileBase[width, height];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var candidates = GetAllCandidates(tileset);

            // Filtrar por vecinos ya colapsados
            if (y - 1 >= 0) // norte
                FilterByNeighbor(candidates, tileset, result[x, y - 1], dir: 0);
            if (x + 1 < width) // este (todavía sin asignar, no filtra)
            { }
            if (y + 1 < height) // sur (sin asignar)
            { }
            if (x - 1 >= 0) // oeste
                FilterByNeighbor(candidates, tileset, result[x - 1, y], dir: 3);

            // Elegir ponderado
            var chosen = ChooseWeighted(candidates, rng);
            result[x, y] = chosen?.tile ?? tileset.modules[0].tile;
        }

        return result;
    }

    private static List<WFCTileset.TileModule> GetAllCandidates(WFCTileset tileset)
        => new List<WFCTileset.TileModule>(tileset.modules);

    // dir: 0=norte,1=este,2=sur,3=oeste; neighborTile es el TileBase ya asignado en ese lado.
    private static void FilterByNeighbor(List<WFCTileset.TileModule> candidates, WFCTileset tileset, TileBase neighborTile, int dir)
    {
        if (neighborTile == null) return;
        // Buscar el módulo del vecino por TileBase
        WFCTileset.TileModule neighbor = null;
        foreach (var m in tileset.modules)
        {
            if (m.tile == neighborTile) { neighbor = m; break; }
        }
        if (neighbor == null) return;

        // Qué IDs permite el vecino hacia el lado opuesto
        List<string> allowedIdsFromNeighbor = null;
        switch (dir)
        {
            case 0: // el vecino está al norte; su lado sur debe permitirnos
                allowedIdsFromNeighbor = neighbor.south; break;
            case 1: // vecino al este; su oeste debe permitirnos
                allowedIdsFromNeighbor = neighbor.west; break;
            case 2: // vecino al sur; su norte debe permitirnos
                allowedIdsFromNeighbor = neighbor.north; break;
            case 3: // vecino al oeste; su este debe permitirnos
                allowedIdsFromNeighbor = neighbor.east; break;
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