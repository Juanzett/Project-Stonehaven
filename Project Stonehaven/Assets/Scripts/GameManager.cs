using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player")]
    public GameObject player;

    [Header("World")]
    public WorldGenerator worldGenerator;
    [Tooltip("Si es 0, se genera aleatorio.")]
    public int seed;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (seed == 0)
            seed = Random.Range(int.MinValue, int.MaxValue);
    }

    void Start()
    {
        if (worldGenerator != null)
        {
            worldGenerator.Generate(seed);
        }
    }

    public void Regenerate(int newSeed)
    {
        seed = newSeed;
        if (worldGenerator != null)
            worldGenerator.Generate(seed);
    }
}
