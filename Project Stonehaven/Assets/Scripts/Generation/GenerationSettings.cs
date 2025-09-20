using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenerationSettings", menuName = "World/Generation Settings")]
public class GenerationSettings : ScriptableObject
{
    [Header("Dimensions (tiles)")]
    public int mapWidth = 512;
    public int mapHeight = 512;
    public int chunkSize = 64;

    [Header("Tiles")]
    [Tooltip("Tama�o del tile en p�xeles (arte). No afecta la generaci�n.")]
    public int tilePixelSize = 32;

    [Header("Seed")]
    public int seed = 0;

    [Header("Biomes")]
    public List<BiomeConfig> biomes = new List<BiomeConfig>();
}