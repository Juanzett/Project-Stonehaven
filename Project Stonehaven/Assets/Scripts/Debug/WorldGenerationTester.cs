using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerationTester : MonoBehaviour
{
    [Header("Referencias")]
    public GameManager gameManager;
    public WorldGenerator worldGenerator;
    public WFCTileset tileset; // El mismo usado en WorldGenerator

    [Header("Input")]
    public KeyCode regenerateKey = KeyCode.R;
    public bool newSeedEachTime = true;

    [Header("Decoración")]
    public bool spawnRocks = true;
    public List<GameObject> rockPrefabs = new List<GameObject>();
    public int rockInstances = 40;
    public float rockYOffset = 0f;
    public string grassMatch = "grass"; // patrón en ID para considerar pasto

    [Header("Debug")]
    public bool logStats = true;
    public bool logFirstTime = true;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Dictionary<TileBase, string> tileToId;
    private HashSet<TileBase> waterTiles;
    private HashSet<TileBase> grassTiles;

    void Awake()
    {
        if (gameManager == null) gameManager = GameManager.instance;
        if (worldGenerator == null && gameManager != null)
            worldGenerator = gameManager.worldGenerator;
        BuildTileDictionaries();
    }

    void Start()
    {
        // Log inicial si ya estaba generado
        if (logFirstTime && worldGenerator != null && tileset != null)
        {
            LogWorldStats();
            SpawnRocksIfNeeded();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            Regenerate();
        }
    }

    void BuildTileDictionaries()
    {
        tileToId = new Dictionary<TileBase, string>();
        waterTiles = new HashSet<TileBase>();
        grassTiles = new HashSet<TileBase>();

        if (tileset == null) return;
        foreach (var m in tileset.modules)
        {
            if (m.tile == null) continue;
            tileToId[m.tile] = m.id;

            var idLower = m.id.ToLowerInvariant();
            if (idLower.Contains("water"))
                waterTiles.Add(m.tile);
            if (idLower.Contains(grassMatch.ToLowerInvariant()))
                grassTiles.Add(m.tile);
        }
    }

    public void Regenerate()
    {
        if (gameManager == null || worldGenerator == null)
        {
            Debug.LogWarning("WorldGenerationTester: faltan referencias (GameManager / WorldGenerator).");
            return;
        }

        if (newSeedEachTime)
        {
            int newSeed = Random.Range(int.MinValue, int.MaxValue);
            gameManager.Regenerate(newSeed);
        }
        else
        {
            gameManager.Regenerate(gameManager.seed);
        }

        // Rebuild mapping por si el tileset fue cambiado
        BuildTileDictionaries();

        // Post generación
        LogWorldStats();
        ClearRocks();
        SpawnRocksIfNeeded();
    }

    void LogWorldStats()
    {
        if (!logStats || worldGenerator == null || worldGenerator.tilemap == null) return;

        var tm = worldGenerator.tilemap;
        int w = worldGenerator.settings.mapWidth;
        int h = worldGenerator.settings.mapHeight;

        int waterCount = 0;
        int grassCount = 0;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var t = tm.GetTile(new Vector3Int(x, y, 0));
            if (t == null) continue;
            if (waterTiles.Contains(t)) waterCount++;
            else if (grassTiles.Contains(t)) grassCount++;
        }

        Debug.Log($"[WorldGenerationTester] Seed={gameManager.seed} WaterTiles={waterCount} GrassTiles={grassCount}");
        if (waterCount == 0)
        {
            Debug.LogWarning("[WorldGenerationTester] No se generó agua. Revisa BiomeConfig (isWaterBiome / allowLakes) y thresholds.");
        }
    }

    void SpawnRocksIfNeeded()
    {
        if (!spawnRocks || rockPrefabs.Count == 0 || worldGenerator == null) return;

        var tm = worldGenerator.tilemap;
        int w = worldGenerator.settings.mapWidth;
        int h = worldGenerator.settings.mapHeight;

        // Recolectar celdas elegibles (grass)
        var candidates = new List<Vector3Int>();
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var t = tm.GetTile(new Vector3Int(x, y, 0));
            if (t != null && grassTiles.Contains(t))
                candidates.Add(new Vector3Int(x, y, 0));
        }

        if (candidates.Count == 0) return;

        int spawnCount = Mathf.Min(rockInstances, candidates.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            var cell = candidates[Random.Range(0, candidates.Count)];
            var prefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];
            if (prefab == null) continue;

            Vector3 worldPos = worldGenerator.tilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f + rockYOffset, 0f);
            var inst = Instantiate(prefab, worldPos, Quaternion.identity);
            inst.name = "RockSpawned_" + i;
            spawned.Add(inst);
        }
    }

    void ClearRocks()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                DestroyImmediate(spawned[i]);
        }
        spawned.Clear();
    }
}