using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WFCTileset", menuName = "World/WFC Tileset")]
public class WFCTileset : ScriptableObject
{
    [System.Serializable]
    public enum Category { Center, Edge, Corner }

    [System.Serializable]
    public class TileModule
    {
        [Tooltip("ID único siguiendo patrón: base_center | base_edge_n | base_corner_ne, etc.")]
        public string id = "grass_center";
        public Category category = Category.Center;
        public TileBase tile;
        [Range(0.01f, 10f)] public float weight = 1f;

        [Header("Adjacency IDs (legacy, se regeneran)")]
        public List<string> north = new List<string>();
        public List<string> east  = new List<string>();
        public List<string> south = new List<string>();
        public List<string> west  = new List<string>();

        [Header("Sockets auto (no editar a mano)")]
        public string socketN;
        public string socketE;
        public string socketS;
        public string socketW;
    }

    public List<TileModule> modules = new List<TileModule>();

    private Dictionary<string, TileModule> _byId;
    public TileModule Get(string id)
    {
        if (_byId == null)
        {
            _byId = new Dictionary<string, TileModule>();
            foreach (var m in modules)
                if (!_byId.ContainsKey(m.id))
                    _byId[m.id] = m;
        }
        _byId.TryGetValue(id, out var mod);
        return mod;
    }
}