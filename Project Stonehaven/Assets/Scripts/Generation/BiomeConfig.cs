using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Biome", menuName = "World/Biome")]
public class BiomeConfig : ScriptableObject
{
    public string biomeId = "forest";

    [Header("Noise")]
    [Range(0.001f, 1f)] public float perlinScale = 0.05f;
    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0f, 1f)] public float weight = 1f;

    [Header("Rendering (fallback)")]
    public TileBase defaultTile;
}