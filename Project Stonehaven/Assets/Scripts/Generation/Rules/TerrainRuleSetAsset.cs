using UnityEngine;

public static class TerrainRuleSetAsset
{
    public static TerrainRuleSet Create(string assetPath)
    {
        var instance = ScriptableObject.CreateInstance<TerrainRuleSet>();
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
        return instance;
    }
}