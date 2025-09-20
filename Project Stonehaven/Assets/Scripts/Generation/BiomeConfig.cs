using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Biome", menuName = "World/Biome")]
public class BiomeConfig : ScriptableObject
{
    public string biomeId = "forest";

    [Header("Noise (selección de bioma)")]
    [Range(0.001f, 1f)] public float perlinScale = 0.05f;
    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0f, 1f)] public float weight = 1f;

    [Header("Rendering (fallback)")]
    public TileBase defaultTile;

    [Header("Agua por bioma")]
    [Tooltip("Si este bioma ES agua (mar, río, pantano).")]
    public bool isWaterBiome = false;

    [Tooltip("Si este bioma permite lagos/estanques locales.")]
    public bool allowLakes = false;

    [Range(0.001f, 1f)]
    [Tooltip("Escala del ruido para lagos locales.")]
    public float lakeNoiseScale = 0.03f;

    [Range(0f, 1f)]
    [Tooltip("Umbral del ruido para formar lago. Más alto = menos lagos.")]
    public float lakeThreshold = 0.75f;
}