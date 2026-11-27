using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time editor operation: save larger inspector scales on the actual prefabs.
public static class ResizeCityBuildings
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/IntegratedCity.unity");
        var construction = Object.FindFirstObjectByType<CityConstruction>();
        if (construction == null || construction.Cards == null) throw new System.InvalidOperationException("Missing city building cards.");
        var paths = new HashSet<string>();
        var cards = new HashSet<BuildingCard>();
        foreach (var card in construction.Cards)
        {
            if (card == null || card.completPrefab == null || card.meshPrefab == null || card.placementRule == null) continue;
            cards.Add(card);
            paths.Add(AssetDatabase.GetAssetPath(card.completPrefab));
            paths.Add(AssetDatabase.GetAssetPath(card.meshPrefab));
        }
        if (cards.Count > 0 && System.Linq.Enumerable.All(cards, card => card.width >= 4 && card.height >= 4 && card.width % 4 == 0 && card.height % 4 == 0))
            throw new System.InvalidOperationException("The building cards already appear to have been resized; refusing to apply 4x again.");
        foreach (string path in paths)
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) throw new System.InvalidOperationException("Expected a prefab: " + path);
        foreach (string path in paths)
        {
            var prefab = PrefabUtility.LoadPrefabContents(path);
            prefab.transform.localScale *= 4f;
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);
        }
        foreach (var card in cards)
        {
            int oldWidth = card.placementRule is FourSlotPlacementRules ? 2 : Mathf.Max(1, card.width);
            int oldHeight = card.placementRule is FourSlotPlacementRules ? 2 : Mathf.Max(1, card.height);
            card.width = oldWidth * 4;
            card.height = oldHeight * 4;
            EditorUtility.SetDirty(card);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"CITY_BUILDINGS_RESIZED: {paths.Count} prefabs, {cards.Count} cards");
    }
}
