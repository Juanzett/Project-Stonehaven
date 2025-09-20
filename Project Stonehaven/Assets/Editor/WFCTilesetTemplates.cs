using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class WFCTilesetTemplates
{
    [MenuItem("Assets/Create/World/WFC Tileset (Plantilla Grass/Water)")]
    public static void CreateGrassWaterTemplate()
    {
        var asset = ScriptableObject.CreateInstance<WFCTileset>();
        var mods = new List<WFCTileset.TileModule>();

        WFCTileset.TileModule M(string id, WFCTileset.Category cat)
        {
            return new WFCTileset.TileModule
            {
                id = id,
                category = cat,
                weight = 1f,
                north = new List<string>(),
                east = new List<string>(),
                south = new List<string>(),
                west = new List<string>()
            };
        }

        // IDs
        const string G = "grass_center";
        const string W = "water_center";
        const string EN = "grass_edge_n";
        const string EE = "grass_edge_e";
        const string ES = "grass_edge_s";
        const string EW = "grass_edge_w";
        const string CNE = "grass_corner_ne";
        const string CNW = "grass_corner_nw";
        const string CSE = "grass_corner_se";
        const string CSW = "grass_corner_sw";

        // Helpers
        void N(WFCTileset.TileModule m, params string[] ids) => m.north.AddRange(ids);
        void E(WFCTileset.TileModule m, params string[] ids) => m.east.AddRange(ids);
        void S(WFCTileset.TileModule m, params string[] ids) => m.south.AddRange(ids);
        void Wt(WFCTileset.TileModule m, params string[] ids) => m.west.AddRange(ids);

        // 1) Centers
        var grass = M(G, WFCTileset.Category.Center);
        N(grass, G, EN, CNE, CNW);
        E(grass, G, EE, CNE, CSE);
        S(grass, G, ES, CSE, CSW);
        Wt(grass, G, EW, CNW, CSW);
        mods.Add(grass);

        var water = M(W, WFCTileset.Category.Center);
        N(water, W, ES, CSE, CSW);
        E(water, W, EW, CSW, CNW);
        S(water, W, EN, CNE, CNW);
        Wt(water, W, EE, CNE, CSE);
        mods.Add(water);

        // 2) Edges (césped con agua al lado indicado)
        var edgeN = M(EN, WFCTileset.Category.Edge); // agua al norte
        N(edgeN, W);
        E(edgeN, EN, CNE);
        S(edgeN, G, EN, CNE, CNW);
        Wt(edgeN, EN, CNW);
        mods.Add(edgeN);

        var edgeS = M(ES, WFCTileset.Category.Edge); // agua al sur
        S(edgeS, W);
        E(edgeS, ES, CSE);
        N(edgeS, G, ES, CSE, CSW);
        Wt(edgeS, ES, CSW);
        mods.Add(edgeS);

        var edgeE = M(EE, WFCTileset.Category.Edge); // agua al este
        E(edgeE, W);
        N(edgeE, EE, CNE);
        Wt(edgeE, G, EE, CNE, CSE);
        S(edgeE, EE, CSE);
        mods.Add(edgeE);

        var edgeW = M(EW, WFCTileset.Category.Edge); // agua al oeste
        Wt(edgeW, W);
        N(edgeW, EW, CNW);
        E(edgeW, G, EW, CNW, CSW);
        S(edgeW, EW, CSW);
        mods.Add(edgeW);

        // 3) Corners (césped con agua en dos lados)
        var cornerNE = M(CNE, WFCTileset.Category.Corner); // agua al norte y este
        N(cornerNE, W);
        E(cornerNE, W);
        S(cornerNE, G, EN, CSE);
        Wt(cornerNE, G, EE, CNW);
        mods.Add(cornerNE);

        var cornerNW = M(CNW, WFCTileset.Category.Corner); // agua al norte y oeste
        N(cornerNW, W);
        Wt(cornerNW, W);
        S(cornerNW, G, EN, CSW);
        E(cornerNW, G, EW, CNE);
        mods.Add(cornerNW);

        var cornerSE = M(CSE, WFCTileset.Category.Corner); // agua al sur y este
        S(cornerSE, W);
        E(cornerSE, W);
        N(cornerSE, G, ES, CNE);
        Wt(cornerSE, G, EE, CSW);
        mods.Add(cornerSE);

        var cornerSW = M(CSW, WFCTileset.Category.Corner); // agua al sur y oeste
        S(cornerSW, W);
        Wt(cornerSW, W);
        N(cornerSW, G, ES, CNW);
        E(cornerSW, G, EW, CSE);
        mods.Add(cornerSW);

        asset.modules = mods;

        // Crear asset
        string path = EditorUtility.SaveFilePanelInProject(
            "Crear WFC Tileset (Grass/Water)",
            "WFCTileset_GrassWater",
            "asset",
            "Elige dónde guardar el WFCTileset."
        );
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}