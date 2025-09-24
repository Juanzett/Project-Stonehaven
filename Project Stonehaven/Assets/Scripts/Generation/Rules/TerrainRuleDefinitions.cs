using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum SeasonVariant
{
    Spring,   // pasto amarillento
    Summer    // pasto verde
}

public enum TerrainCategory
{
    GrassSpring,
    GrassSummer,
    Water,
    Shore,        // transición agua-tierra
    Decoration,   // flores, rocas, etc. (no afecta sockets)
    Structure     // casas, muelles, etc. (no afecta sockets)
}

public enum WaterShapeKind
{
    None,
    Line,
    Curve,
    Corner,
    Tee,
    Cross,
    SmallLake,
    MediumLake,
    LargeLake,
    SShape,
    Block2x2,
    Block3x3,
    Irregular
}

public enum CellClassification
{
    // Unitaria
    Single,

    // Bordes cardinales
    EdgeN,
    EdgeE,
    EdgeS,
    EdgeW,

    // Esquinas
    CornerNE,
    CornerNW,
    CornerSE,
    CornerSW,

    // Centros y variantes interiores
    Center,
    CenterN,
    CenterE,
    CenterS,
    CenterW,
    CenterNE,
    CenterNW,
    CenterSE,
    CenterSW,

    // Celda vacía (no exporta módulo, pero mantiene geometría del patrón)
    Empty
}

[Serializable]
public class CellRule
{
    public int x;
    public int y;
    public Sprite sprite;
    public CellClassification classification = CellClassification.Center;
    public string tag;
}

[Serializable]
public class TerrainRuleGroup
{
    public string groupName = "NuevoGrupo";
    public SeasonVariant season;
    public TerrainCategory category;
    public WaterShapeKind shapeKind = WaterShapeKind.None;

    [Range(1,3)] public int width = 1;
    [Range(1,3)] public int height = 1;

    [Tooltip("ID base opcional. Si está vacío se genera automáticamente.")]
    public string baseId;

    [Tooltip("Si es un patrón multi-celda (2x2 / 3x3).")]
    public bool isMultiTile;

    [Tooltip("Marcar si esto representa una pieza de transición agua-tierra.")]
    public bool isShore;

    [Tooltip("Celdas que componen el patrón.")]
    public List<CellRule> cells = new List<CellRule>();
}

public class TerrainRuleSet : ScriptableObject
{
    public List<TerrainRuleGroup> groups = new List<TerrainRuleGroup>();
}