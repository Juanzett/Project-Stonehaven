    using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class WFCTilesetAutoBuilder
{
    // Regex: base_tipo(_dirs)?
    static readonly Regex Pattern = new Regex(@"^(?<base>[a-zA-Z0-9]+)_(?<kind>center|edge|corner)(?:_(?<dirs>[nesw]{1,2}))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Mapeo "terreno base" -> "terreno exterior" (para edges/corners)
    // Ajusta aquí si agregas más combinaciones (ej: grass->water, sand->water, water->grass).
    static readonly Dictionary<string, string> ExteriorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "grass", "water" },
        { "water", "grass" }
    };

    [MenuItem("Assets/World/WFC/Rebuild Sockets & Adjacency (Seleccionar Tileset)")]
    public static void RebuildSelected()
    {
        var obj = Selection.activeObject as WFCTileset;
        if (obj == null)
        {
            Debug.LogError("Selecciona un WFCTileset antes de ejecutar.");
            return;
        }
        Rebuild(obj);
        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssets();
        Debug.Log("WFCTileset reconstruido: " + obj.name);
    }

    public static void Rebuild(WFCTileset tileset)
    {
        if (tileset.modules == null) return;

        // 1) Detectar center modules para posibles bases
        var basesDetected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in tileset.modules)
        {
            var match = Pattern.Match(m.id);
            if (!match.Success) continue;
            var baseName = match.Groups["base"].Value;
            var kind = match.Groups["kind"].Value;
            if (kind == "center")
                basesDetected.Add(baseName);
        }

        // 2) Asignar sockets
        foreach (var mod in tileset.modules)
        {
            mod.north.Clear();
            mod.east.Clear();
            mod.south.Clear();
            mod.west.Clear();

            mod.socketN = mod.socketE = mod.socketS = mod.socketW = "";

            var match = Pattern.Match(mod.id);
            if (!match.Success)
            {
                Debug.LogWarning("ID no coincide con patrón esperado: " + mod.id);
                continue;
            }

            var baseName = match.Groups["base"].Value.ToLowerInvariant();
            var kind = match.Groups["kind"].Value.ToLowerInvariant();
            var dirs = match.Groups["dirs"].Success ? match.Groups["dirs"].Value.ToLowerInvariant() : "";

            // Terreno interior = baseName
            // Terreno exterior (si existe): buscamos mapeo
            string exterior;
            if (!ExteriorMap.TryGetValue(baseName, out exterior))
            {
                // fallback: usar primer base distinto encontrado
                exterior = null;
                foreach (var b in basesDetected)
                {
                    if (!b.Equals(baseName, StringComparison.OrdinalIgnoreCase))
                    {
                        exterior = b;
                        break;
                    }
                }
                if (exterior == null) exterior = baseName; // si no hay otro, homogéneo
            }

            switch (kind)
            {
                case "center":
                    mod.socketN = mod.socketE = mod.socketS = mod.socketW = baseName;
                    break;

                case "edge":
                    // dirs = una letra (n/e/s/w) que indica dónde está el exterior
                    mod.socketN = baseName;
                    mod.socketE = baseName;
                    mod.socketS = baseName;
                    mod.socketW = baseName;
                    if (dirs.Length == 1)
                    {
                        switch (dirs[0])
                        {
                            case 'n': mod.socketN = exterior; break;
                            case 'e': mod.socketE = exterior; break;
                            case 's': mod.socketS = exterior; break;
                            case 'w': mod.socketW = exterior; break;
                        }
                    }
                    break;

                case "corner":
                    // dirs = dos letras (ej: ne) lados exteriores
                    mod.socketN = baseName;
                    mod.socketE = baseName;
                    mod.socketS = baseName;
                    mod.socketW = baseName;
                    if (dirs.Length == 2)
                    {
                        foreach (var c in dirs)
                        {
                            switch (c)
                            {
                                case 'n': mod.socketN = exterior; break;
                                case 'e': mod.socketE = exterior; break;
                                case 's': mod.socketS = exterior; break;
                                case 'w': mod.socketW = exterior; break;
                            }
                        }
                    }
                    break;
            }
        }

        // 3) Construir listas de adyacencia a partir de sockets
        // Compatible si socket lado A == socket lado opuesto B
        foreach (var a in tileset.modules)
        {
            foreach (var b in tileset.modules)
            {
                if (a == b) { /* puede auto-adjuntarse si sockets coinciden */ }

                // NORTH: vecino arriba de A ? su SOUTH debe coincidir
                if (a.socketN == b.socketS) a.north.Add(b.id);
                if (a.socketE == b.socketW) a.east.Add(b.id);
                if (a.socketS == b.socketN) a.south.Add(b.id);
                if (a.socketW == b.socketE) a.west.Add(b.id);
            }
        }
    }
}