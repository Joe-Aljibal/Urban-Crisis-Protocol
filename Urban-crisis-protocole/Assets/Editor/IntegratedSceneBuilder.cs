using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Builds a separate playable starting point while retaining the source scenes.
public static class IntegratedSceneBuilder
{
    public static void Build()
    {
        const string source = "Assets/Ara_Scripts/UiDesign/NewWorldDesign/NewWorldDesign.unity";
        const string destination = "Assets/Scenes/IntegratedCity.unity";
        var scene = EditorSceneManager.OpenScene(source);
        var construction = Object.FindFirstObjectByType<CityConstruction>();
        if (construction == null) throw new System.InvalidOperationException("City construction is missing.");
        var cards = AssetDatabase.FindAssets("t:BuildingCard")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<BuildingCard>)
            .Where(card => card != null && card.completPrefab != null && card.meshPrefab != null &&
                           card.prefabCurrentCaseSelection != null && card.placementRule != null &&
                           card.completPrefab.GetComponent<IBuilding>() != null)
            .OrderBy(card => card.name).ToArray();
        var serialized = new SerializedObject(construction);
        var array = serialized.FindProperty("cards");
        array.arraySize = cards.Length;
        for (int i = 0; i < cards.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var mainUi = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Additive);
        foreach (var root in mainUi.GetRootGameObjects())
        {
            var loader = root.GetComponent<GameWorldLoader>();
            if (loader != null) loader.enabled = false;
            SceneManager.MoveGameObjectToScene(root, scene);
        }
        EditorSceneManager.CloseScene(mainUi, true);
        SceneManager.SetActiveScene(scene);
        new GameObject("Integrated Game Controls").AddComponent<IntegratedGameControls>();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, destination, true))
            throw new System.InvalidOperationException("Could not save integrated scene.");
        if (!EditorBuildSettings.scenes.Any(entry => entry.path == destination))
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(destination, true) }).ToArray();
        Debug.Log($"INTEGRATED_SCENE_CREATED: {cards.Length} building cards");
    }
}
