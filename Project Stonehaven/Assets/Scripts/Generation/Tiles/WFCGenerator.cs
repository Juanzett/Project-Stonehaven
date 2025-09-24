using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WFCGenerator
{
    private const int MaxRetries = 8;

    private class ModuleData
    {
        public WFCTileset.TileModule module;
        public List<int> north = new List<int>();
        public List<int> east  = new List<int>();
        public List<int> south = new List<int>();
        public List<int> west  = new List<int>();
        public float weight;
    }

    public static TileBase[,] Generate(
        WFCTileset tileset,
        int width,
        int height,
        System.Random rng,
        List<string>[,] allowedPerCell)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            var result = TryGenerate(tileset, width, height, rng, allowedPerCell);
            if (result != null)
                return result;
        }

        Debug.LogWarning("WFC: Fallback tras varios intentos.");
        var fb = new TileBase[width, height];
        if (tileset.modules.Count > 0)
        {
            var t = tileset.modules[0].tile;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    fb[x, y] = t;
        }
        return fb;
    }

    public static TileBase[,] Generate(WFCTileset tileset, int width, int height, System.Random rng)
        => Generate(tileset, width, height, rng, null);

    private static TileBase[,] TryGenerate(
        WFCTileset tileset,
        int width,
        int height,
        System.Random rng,
        List<string>[,] allowedPerCell)
    {
        if (tileset == null || tileset.modules.Count == 0)
            throw new ArgumentException("WFCTileset vacío");

        var modules = BuildModuleData(tileset);
        int moduleCount = modules.Count;

        // possible[x,y,i]
        var possible = new bool[width, height, moduleCount];
        var counts   = new int[width, height];

        // Inicialización
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            counts[x, y] = moduleCount;
            for (int i = 0; i < moduleCount; i++)
                possible[x, y, i] = true;

            if (allowedPerCell != null)
            {
                var allowed = allowedPerCell[x, y];
                if (allowed != null && allowed.Count > 0)
                {
                    int removed = 0;
                    for (int i = 0; i < moduleCount; i++)
                    {
                        if (!allowed.Contains(modules[i].module.id))
                        {
                            possible[x, y, i] = false;
                            removed++;
                        }
                    }
                    counts[x, y] = moduleCount - removed;
                    if (counts[x, y] <= 0) return null;
                }
            }
        }

        var queue = new Queue<(int x, int y)>();
        int totalCells = width * height;
        int collapsed = 0;

        while (collapsed < totalCells)
        {
            if (!SelectMinEntropyCell(width, height, counts, possible, moduleCount, rng, out int sx, out int sy))
                break;

            int chosen = PickWeighted(sx, sy, possible, modules, moduleCount, rng);
            if (chosen < 0) return null;

            // Colapsar celda
            for (int i = 0; i < moduleCount; i++)
                if (i != chosen && possible[sx, sy, i])
                    possible[sx, sy, i] = false;

            counts[sx, sy] = 1;
            collapsed++;
            queue.Enqueue((sx, sy));

            // Propagación
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                Propagate(cx, cy, cx, cy + 1, 0, modules, possible, counts, moduleCount, queue, width, height); // N
                Propagate(cx, cy, cx + 1, cy, 1, modules, possible, counts, moduleCount, queue, width, height); // E
                Propagate(cx, cy, cx, cy - 1, 2, modules, possible, counts, moduleCount, queue, width, height); // S
                Propagate(cx, cy, cx - 1, cy, 3, modules, possible, counts, moduleCount, queue, width, height); // W
            }
        }

        // Construir resultado
        var output = new TileBase[width, height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int idx = FirstTrue(possible, x, y, moduleCount);
            if (idx < 0) return null;
            output[x, y] = modules[idx].module.tile;
        }
        return output;
    }

    private static List<ModuleData> BuildModuleData(WFCTileset tileset)
    {
        var list = new List<ModuleData>(tileset.modules.Count);
        var idIndex = new Dictionary<string, int>(StringComparer.Ordinal);

        for (int i = 0; i < tileset.modules.Count; i++)
        {
            var m = tileset.modules[i];
            list.Add(new ModuleData
            {
                module = m,
                weight = m.weight
            });
            idIndex[m.id] = i;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var md = list[i];
            AddAdj(md.north, md.module.north, idIndex);
            AddAdj(md.east,  md.module.east,  idIndex);
            AddAdj(md.south, md.module.south, idIndex);
            AddAdj(md.west,  md.module.west,  idIndex);
        }
        return list;
    }

    private static void AddAdj(List<int> target, List<string> ids, Dictionary<string, int> idIndex)
    {
        for (int i = 0; i < ids.Count; i++)
            if (idIndex.TryGetValue(ids[i], out int idx))
                target.Add(idx);
    }

    private static bool SelectMinEntropyCell(
        int width,
        int height,
        int[,] counts,
        bool[,,] possible,
        int moduleCount,
        System.Random rng,
        out int sx,
        out int sy)
    {
        sx = sy = -1;
        int best = int.MaxValue;
        int ties = 0;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int c = counts[x, y];
            if (c > 1 && c < best)
            {
                best = c; sx = x; sy = y; ties = 1;
            }
            else if (c > 1 && c == best)
            {
                ties++;
                if (rng.NextDouble() < 1.0 / ties)
                {
                    sx = x; sy = y;
                }
            }
        }
        return sx != -1;
    }

    private static int PickWeighted(
        int x,
        int y,
        bool[,,] possible,
        List<ModuleData> modules,
        int moduleCount,
        System.Random rng)
    {
        float total = 0f;
        for (int i = 0; i < moduleCount; i++)
            if (possible[x, y, i])
                total += modules[i].weight;

        if (total <= 0f) return -1;

        float r = (float)(rng.NextDouble() * total);
        for (int i = 0; i < moduleCount; i++)
        {
            if (!possible[x, y, i]) continue;
            if (r < modules[i].weight) return i;
            r -= modules[i].weight;
        }
        for (int i = 0; i < moduleCount; i++)
            if (possible[x, y, i]) return i;
        return -1;
    }

    // direction: 0=N,1=E,2=S,3=W
    private static void Propagate(
        int ox, int oy,
        int dx, int dy,
        int direction,
        List<ModuleData> modules,
        bool[,,] possible,
        int[,] counts,
        int moduleCount,
        Queue<(int x, int y)> queue,
        int width,
        int height)
    {
        if (dx < 0 || dy < 0 || dx >= width || dy >= height) return;

        bool changed = false;

        for (int dIdx = 0; dIdx < moduleCount; dIdx++)
        {
            if (!possible[dx, dy, dIdx]) continue;

            bool supported = false;

            for (int oIdx = 0; oIdx < moduleCount && !supported; oIdx++)
            {
                if (!possible[ox, oy, oIdx]) continue;

                List<int> allowedList = null;
                var oMod = modules[oIdx];
                switch (direction)
                {
                    case 0: allowedList = oMod.north; break;
                    case 1: allowedList = oMod.east;  break;
                    case 2: allowedList = oMod.south; break;
                    case 3: allowedList = oMod.west;  break;
                }
                if (allowedList != null && allowedList.Contains(dIdx))
                    supported = true;
            }

            if (!supported)
            {
                possible[dx, dy, dIdx] = false;
                changed = true;
            }
        }

        if (changed)
        {
            int count = 0;
            for (int i = 0; i < moduleCount; i++)
                if (possible[dx, dy, i]) count++;

            counts[dx, dy] = count;
            if (count == 0) return; // contradicción: reintento global
            queue.Enqueue((dx, dy));
        }
    }

    private static int FirstTrue(bool[,,] possible, int x, int y, int moduleCount)
    {
        for (int i = 0; i < moduleCount; i++)
            if (possible[x, y, i]) return i;
        return -1;
    }
}