using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DoubleCityBuildings
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/IntegratedCity.unity");
        var construction = Object.FindFirstObjectByType<CityConstruction>();
        if (construction == null || construction.Cards == null) throw new System.InvalidOperationException("Missing building cards.");
        var cards = new HashSet<BuildingCard>();
        var paths = new HashSet<string>();
        foreach (var card in construction.Cards)
        {
            if (card == null || card.completPrefab == null || card.meshPrefab == null || card.placementRule == null) continue;
            cards.Add(card);
            paths.Add(AssetDatabase.GetAssetPath(card.completPrefab));
            paths.Add(AssetDatabase.GetAssetPath(card.meshPrefab));
        }
        if (cards.Count != 20 || paths.Count != 36) throw new System.InvalidOperationException("Unexpected building assets; no changes made.");
        if (cards.All(card => card.width >= 8 && card.height >= 8 && card.width % 8 == 0 && card.height % 8 == 0))
            throw new System.InvalidOperationException("Buildings already appear doubled.");
        foreach (var path in paths)
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) throw new System.InvalidOperationException("Invalid prefab path: " + path);
        foreach (var path in paths)
        {
            var prefab = PrefabUtility.LoadPrefabContents(path);
            prefab.transform.localScale *= 2f;
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);
        }
        foreach (var card in cards)
        {
            card.width *= 2;
            card.height *= 2;
            EditorUtility.SetDirty(card);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"CITY_BUILDINGS_DOUBLED: {paths.Count} prefabs, {cards.Count} cards");
    }
}
