using System;
using System.Collections.Generic;
using System.Linq; // NEW
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainRuleEditorWindow : EditorWindow
{
    private TerrainRuleSet ruleSet;
    private Vector2 scroll;
    private int selectedGroupIndex = -1;

    // Export
    private WFCTileset exportTileset;
    private string exportTilesetPath = "Assets/WFCTileset_Auto.asset";

    [MenuItem("Tools/Terrain Rules/Editor")]
    public static void Open()
    {
        GetWindow<TerrainRuleEditorWindow>("Terrain Rules");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        DrawRuleSetSelection();

        if (ruleSet == null)
        {
            EditorGUILayout.HelpBox("Crea o asigna un TerrainRuleSet.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        DrawGroupsList();

        if (selectedGroupIndex >= 0 && selectedGroupIndex < ruleSet.groups.Count)
        {
            EditorGUILayout.Space();
            var group = ruleSet.groups[selectedGroupIndex];
            DrawGroupEditor(group);
        }

        EditorGUILayout.Space(20);
        DrawExportSection();
    }

    private void DrawRuleSetSelection()
    {
        EditorGUILayout.BeginHorizontal();
        ruleSet = (TerrainRuleSet)EditorGUILayout.ObjectField("Rule Set", ruleSet, typeof(TerrainRuleSet), false);
        if (GUILayout.Button("Nuevo", GUILayout.Width(70)))
        {
            string path = EditorUtility.SaveFilePanelInProject("Crear TerrainRuleSet", "TerrainRuleSet", "asset", "Elige ubicación");
            if (!string.IsNullOrEmpty(path))
            {
                ruleSet = TerrainRuleSetAsset.Create(path);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGroupsList()
    {
        EditorGUILayout.LabelField("Grupos", EditorStyles.boldLabel);
        if (GUILayout.Button("+ Añadir Grupo"))
        {
            var g = new TerrainRuleGroup
            {
                groupName = "Grupo_" + ruleSet.groups.Count,
                width = 1,
                height = 1,
                cells = new List<CellRule>()
            };
            RebuildCells(g);
            ruleSet.groups.Add(g);
            selectedGroupIndex = ruleSet.groups.Count - 1;
            MarkDirty();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(140));
        for (int i = 0; i < ruleSet.groups.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(selectedGroupIndex == i, ruleSet.groups[i].groupName, "Button"))
            {
                selectedGroupIndex = i;
            }
            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                if (EditorUtility.DisplayDialog("Eliminar", "¿Eliminar grupo?", "Sí", "No"))
                {
                    ruleSet.groups.RemoveAt(i);
                    if (selectedGroupIndex == i) selectedGroupIndex = -1;
                    MarkDirty();
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawGroupEditor(TerrainRuleGroup g)
    {
        EditorGUILayout.LabelField("Editar Grupo", EditorStyles.boldLabel);
        g.groupName = EditorGUILayout.TextField("Nombre", g.groupName);
        g.season = (SeasonVariant)EditorGUILayout.EnumPopup("Temporada", g.season);
        g.category = (TerrainCategory)EditorGUILayout.EnumPopup("Categoría", g.category);
        g.shapeKind = (WaterShapeKind)EditorGUILayout.EnumPopup("Forma/Shape", g.shapeKind);

        g.isShore = EditorGUILayout.Toggle("Es transición (shore)", g.isShore);
        g.isMultiTile = EditorGUILayout.Toggle("Multi-Tile", g.isMultiTile);
        EditorGUI.BeginChangeCheck();
        int newW = EditorGUILayout.IntSlider("Width", g.width, 1, 3);
        int newH = EditorGUILayout.IntSlider("Height", g.height, 1, 3);
        if (EditorGUI.EndChangeCheck())
        {
            g.width = newW;
            g.height = newH;
            RebuildCells(g);
            MarkDirty();
        }

        g.baseId = EditorGUILayout.TextField(new GUIContent("Base ID (opcional)", "Si lo dejas vacío se genera automáticamente en export."), g.baseId);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grilla", EditorStyles.boldLabel);

        for (int y = g.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < g.width; x++)
            {
                var cell = g.cells.Find(c => c.x == x && c.y == y);
                GUILayout.BeginVertical("box", GUILayout.Width(110));
                EditorGUILayout.LabelField($"({x},{y})", GUILayout.Width(100));
                cell.sprite = (Sprite)EditorGUILayout.ObjectField(cell.sprite, typeof(Sprite), false, GUILayout.Width(100), GUILayout.Height(64));
                cell.classification = (CellClassification)EditorGUILayout.EnumPopup(cell.classification, GUILayout.Width(100));
                cell.tag = EditorGUILayout.TextField(cell.tag ?? "", GUILayout.Width(100));
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Normalizar IDs sugeridos"))
        {
            MarkDirty();
        }

        if (GUILayout.Button("Clonar Grupo"))
        {
            var clone = JsonUtility.FromJson<TerrainRuleGroup>(JsonUtility.ToJson(g));
            clone.groupName += "_Copy";
            ruleSet.groups.Add(clone);
            selectedGroupIndex = ruleSet.groups.Count - 1;
            MarkDirty();
        }

        if (GUILayout.Button("Eliminar celdas vacías (classification=Empty)"))
        {
            g.cells.RemoveAll(c => c.classification == CellClassification.Empty);
            MarkDirty();
        }
    }

    private void DrawExportSection()
    {
        EditorGUILayout.LabelField("Exportar a WFCTileset", EditorStyles.boldLabel);
        exportTileset = (WFCTileset)EditorGUILayout.ObjectField("WFCTileset destino", exportTileset, typeof(WFCTileset), false);
        exportTilesetPath = EditorGUILayout.TextField("Ruta Asset", exportTilesetPath);

        if (GUILayout.Button("Exportar / Actualizar Tileset"))
        {
            ExportToTileset();
        }
    }

    private void RebuildCells(TerrainRuleGroup g)
    {
        var newList = new List<CellRule>();
        for (int y = 0; y < g.height; y++)
        for (int x = 0; x < g.width; x++)
        {
            var existing = g.cells.Find(c => c.x == x && c.y == y);
            if (existing != null)
                newList.Add(existing);
            else
                newList.Add(new CellRule
                {
                    x = x,
                    y = y,
                    classification = g.width == 1 && g.height == 1
                        ? CellClassification.Single
                        : CellClassification.Center
                });
        }
        g.cells = newList;
    }

    private string SeasonPrefix(SeasonVariant s) => s == SeasonVariant.Spring ? "sp" : "su";

    private string CategoryCode(TerrainCategory c) => c switch
    {
        TerrainCategory.GrassSpring => "grass_sp",
        TerrainCategory.GrassSummer => "grass_su",
        TerrainCategory.Water => "water",
        TerrainCategory.Shore => "shore",
        TerrainCategory.Decoration => "deco",
        TerrainCategory.Structure => "struct",
        _ => "unk"
    };

    private string ShapeCode(WaterShapeKind k) => k switch
    {
        WaterShapeKind.None => "plain",
        WaterShapeKind.Line => "line",
        WaterShapeKind.Curve => "curve",
        WaterShapeKind.Corner => "corner",
        WaterShapeKind.Tee => "tee",
        WaterShapeKind.Cross => "cross",
        WaterShapeKind.SmallLake => "lake_s",
        WaterShapeKind.MediumLake => "lake_m",
        WaterShapeKind.LargeLake => "lake_l",
        WaterShapeKind.SShape => "s_shape",
        WaterShapeKind.Block2x2 => "blk2",
        WaterShapeKind.Block3x3 => "blk3",
        WaterShapeKind.Irregular => "irr",
        _ => "shape"
    };

    private string ClassificationCode(CellClassification c) => c switch
    {
        CellClassification.Single => "center",
        CellClassification.EdgeN => "edge_n",
        CellClassification.EdgeE => "edge_e",
        CellClassification.EdgeS => "edge_s",
        CellClassification.EdgeW => "edge_w",
        CellClassification.CornerNE => "corner_ne",
        CellClassification.CornerNW => "corner_nw",
        CellClassification.CornerSE => "corner_se",
        CellClassification.CornerSW => "corner_sw",
        CellClassification.Center => "center",
        CellClassification.CenterN => "center_n",
        CellClassification.CenterE => "center_e",
        CellClassification.CenterS => "center_s",
        CellClassification.CenterW => "center_w",
        CellClassification.CenterNE => "center_ne",
        CellClassification.CenterNW => "center_nw",
        CellClassification.CenterSE => "center_se",
        CellClassification.CenterSW => "center_sw",
        CellClassification.Empty => "empty",
        _ => "unk"
    };

    private void ExportToTileset()
    {
        if (ruleSet == null)
        {
            Debug.LogError("No RuleSet seleccionado.");
            return;
        }

        if (exportTileset == null)
        {
            exportTileset = ScriptableObject.CreateInstance<WFCTileset>();
            AssetDatabase.CreateAsset(exportTileset, exportTilesetPath);
        }

        var modules = new List<WFCTileset.TileModule>();

        foreach (var g in ruleSet.groups)
        {
            string baseId = string.IsNullOrWhiteSpace(g.baseId)
                ? $"{SeasonPrefix(g.season)}_{CategoryCode(g.category)}_{ShapeCode(g.shapeKind)}"
                : g.baseId;

            foreach (var cell in g.cells)
            {
                if (cell.classification == CellClassification.Empty) continue;
                if (cell.sprite == null)
                {
                    Debug.LogWarning($"Grupo {g.groupName} celda ({cell.x},{cell.y}) sin sprite, omitido.");
                    continue;
                }

                string id = $"{baseId}_{ClassificationCode(cell.classification)}";
                var existing = modules.Find(m => m.id == id);
                if (existing == null)
                {
                    var tm = new WFCTileset.TileModule
                    {
                        id = id,
                        category = ToWFCCategory(cell.classification),
                        tile = GetOrCreateTileAsset(id, cell.sprite),  // UPDATED
                        weight = 1f
                    };
                    modules.Add(tm);
                }
                else
                {
                    // Podrías añadir variantes en futuro aquí
                }
            }
        }

        exportTileset.modules = modules;
        WFCTilesetAutoBuilder.Rebuild(exportTileset);
        EditorUtility.SetDirty(exportTileset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Export completado. Módulos: {modules.Count}");
    }

    private WFCTileset.Category ToWFCCategory(CellClassification c) => c switch
    {
        CellClassification.CornerNE or
        CellClassification.CornerNW or
        CellClassification.CornerSE or
        CellClassification.CornerSW => WFCTileset.Category.Corner,

        CellClassification.EdgeN or
        CellClassification.EdgeE or
        CellClassification.EdgeS or
        CellClassification.EdgeW => WFCTileset.Category.Edge,

        _ => WFCTileset.Category.Center
    };

    // NEW: Crea o reutiliza un Tile sub-asset para persistirlo.
    private TileBase GetOrCreateTileAsset(string id, Sprite sprite)
    {
        if (exportTileset == null || sprite == null) return null;

        string assetPath = AssetDatabase.GetAssetPath(exportTileset);
        if (string.IsNullOrEmpty(assetPath))
            return null;

        // Buscar sub-assets ya cargados
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        // Reusar por nombre
        string tileName = $"tile_{id}";
        var existingTile = subAssets.OfType<Tile>().FirstOrDefault(t => t.name == tileName);
        if (existingTile != null)
        {
            if (existingTile.sprite != sprite)
            {
                existingTile.sprite = sprite;
                EditorUtility.SetDirty(existingTile);
            }
            return existingTile;
        }

        // Crear nuevo tile y agregar como sub-asset
        var newTile = ScriptableObject.CreateInstance<Tile>();
        newTile.name = tileName;
        newTile.sprite = sprite;
        AssetDatabase.AddObjectToAsset(newTile, exportTileset);
        EditorUtility.SetDirty(newTile);
        return newTile;
    }

    private void MarkDirty()
    {
        if (ruleSet != null)
            EditorUtility.SetDirty(ruleSet);
    }
}
