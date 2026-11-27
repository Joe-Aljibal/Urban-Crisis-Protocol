using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class CityMapRegressionCheck
{
    const string Running = "CityMapRegressionCheck.Running";
    static double since; static int step; static Mouse mouse; static float funds; static HutStation producer; static StaticResource resource; static int wood;
    static void InjectMouse() { if (mouse != null && mouse.added) InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right)); }
    static CityMapRegressionCheck() { EditorApplication.update += Update; }
    public static void Run() { EditorSceneManager.OpenScene("Assets/Scenes/IntegratedCity.unity"); SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode(); }
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Update()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
        if (since == 0) since = EditorApplication.timeSinceStartup;
        try
        {
            if (step == 0 && EditorApplication.timeSinceStartup - since > 4)
            {
                var manager = UnityEngine.Object.FindAnyObjectByType<BuildingManager>();
                var surface = UnityEngine.Object.FindAnyObjectByType<CityBuildSurface>();
                var construction = UnityEngine.Object.FindAnyObjectByType<CityConstruction>();
                Check(manager != null && surface != null && surface.Bounds.size.x > 500 && surface.Bounds.size.z > 300, "The build surface must cover the map.");
                var card = AssetDatabase.LoadAssetAtPath<BuildingCard>("Assets/Scripts/Buildings/BuildingPlacemenntSystem/BuildingCard/Houses/Small House.asset");
                var hut = AssetDatabase.LoadAssetAtPath<BuildingCard>("Assets/Scripts/Buildings/BuildingPlacemenntSystem/BuildingCard/WoodHut.asset");
                Check(hut.completPrefab.GetComponent<IBuilding>().GetBuildingSO() != null, "Production card must be configured.");
                foreach (var collector in UnityEngine.Object.FindObjectsByType<SimpleResourceCollector>(FindObjectsSortMode.None)) collector.enabled = false;
                foreach (var collector in UnityEngine.Object.FindObjectsByType<ResourceWorker>(FindObjectsSortMode.None)) collector.enabled = false;
                PlayerInfo.Instance.AddMoney(10000);
                funds = PlayerInfo.Instance.GetMoney;
                Check(!manager.TryPlace(card, new Vector3(surface.Bounds.max.x + 10, 0, 0), Quaternion.identity), "Outside map must be blocked.");
                Check(!manager.TryPlace(card, new Vector3(0, 0, 0), Quaternion.identity), "The river must be blocked.");
                Check(PlayerInfo.Instance.GetMoney == funds, "Invalid placement must not charge.");
                construction.SelectCard(card); Check(manager.PlaceSelectedInBuildArea(), "A valid placement must succeed: " + manager.Status);
                Check(PlayerInfo.Instance.GetMoney == funds - card.completPrefab.GetComponent<IBuilding>().GetPrice(), "Purchase must debit the player city.");
                var house = UnityEngine.Object.FindAnyObjectByType<House>();
                Check(house != null, "Placed house missing.");
                Vector3 existing = house.transform.position;
                construction.SelectCard(card);
                var validate = typeof(BuildingManager).GetMethod("ValidatePlacement", BindingFlags.NonPublic | BindingFlags.Instance);
                Vector3 validPosition = default; bool found = false;
                for (int z = -32; z <= 32 && !found; z += 4)
                    for (int x = -32; x <= 32 && !found; x += 4)
                    {
                        var candidate = existing + new Vector3(x, 0, z);
                        object[] args = { card, candidate, Quaternion.identity, PlayerInfo.Instance, manager.GetComponent<Grid>(), null, Vector3.zero, null };
                        if ((bool)validate.Invoke(manager, args)) { found = true; validPosition = candidate; }
                    }
                Check(found, "A second valid build location must be available.");
                typeof(BuildingManager).GetMethod("InteractWithPreviewBuilding", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { validPosition });
                var validGhost = (GameObject)typeof(BuildingManager).GetField("currentSelectedBuildingClone", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
                var previewMaterials = (Material[])typeof(BuildingManager).GetField("materials", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
                Check(validGhost.GetComponentInChildren<MeshRenderer>(true).material.color == previewMaterials[0].color, "Valid placement must make the preview green.");
                manager.ResetDefaultBuilding();
                funds = PlayerInfo.Instance.GetMoney;
                Check(!manager.TryPlace(card, existing, Quaternion.identity) && PlayerInfo.Instance.GetMoney == funds, "Overlap must be rejected without charge.");
                var other = Cities.cities["AI"];
                float otherFunds = other.GetMoney;
                Check(!manager.TryPlaceForCity(card, existing, Quaternion.identity, other, manager.GetComponent<Grid>()) && other.GetMoney == otherFunds, "Buildings belonging to another city must also block placement.");
                var slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slope.transform.position = new Vector3(500, 0, 500); slope.transform.localScale = new Vector3(20, 1, 20); slope.transform.rotation = Quaternion.Euler(40, 0, 0);
                var testSurfaceObject = new GameObject("Steep test surface");
                var testSurface = testSurfaceObject.AddComponent<CityBuildSurface>();
                typeof(CityBuildSurface).GetField("ground", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(testSurface, slope.GetComponent<Collider>());
                Physics.SyncTransforms();
                Check(!testSurface.Validate(new[] { new Vector3(500, 0, 500) }, .5f, out _, out _), "Steep ground must be blocked.");
                UnityEngine.Object.Destroy(slope); UnityEngine.Object.Destroy(testSurfaceObject);
                PlayerInfo.Instance.AddMoney(-funds);
                Check(!manager.TryPlace(card, new Vector3(-200, 0, 100), Quaternion.identity) && PlayerInfo.Instance.GetMoney == 0, "Insufficient funds must reject placement.");
                construction.SelectCard(card);
                typeof(BuildingManager).GetMethod("InteractWithPreviewBuilding", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { new Vector3(-200, 0, 100) });
                var ghost = (GameObject)typeof(BuildingManager).GetField("currentSelectedBuildingClone", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
                var colors = (Material[])typeof(BuildingManager).GetField("materials", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
                Check(ghost.GetComponentInChildren<MeshRenderer>(true).material.color == colors[1].color, "Insufficient funds must make the preview red.");
                PlayerInfo.Instance.AddMoney(funds);
                construction.SelectCard(card);
                Check(manager.HasSelection, "Preview selection missing.");
                mouse = InputSystem.AddDevice<Mouse>();
                InputSystem.onBeforeUpdate += InjectMouse;
                since = EditorApplication.timeSinceStartup; step = 1; return;
            }
            if (step == 1 && EditorApplication.timeSinceStartup - since > .5)
            {
                var manager = UnityEngine.Object.FindAnyObjectByType<BuildingManager>();
                var ui = UnityEngine.Object.FindAnyObjectByType<JackCityUI>();
                var root = (VisualElement)typeof(JackCityUI).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui);
                Check(root.Q<Label>("MoneyValue").text == "$" + PlayerInfo.Instance.FormattedMoney, "The UI must show the current city balance.");
                typeof(BuildingManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, null);
                Check(!manager.HasSelection && PlayerInfo.Instance.GetMoney == funds, "Right click must cancel without charge: selected=" + manager.HasSelection + " funds=" + PlayerInfo.Instance.GetMoney + " expected=" + funds + " pressed=" + mouse.rightButton.isPressed + " current=" + (Mouse.current == mouse));
                InputSystem.onBeforeUpdate -= InjectMouse; InputSystem.RemoveDevice(mouse); mouse = null;
                var card = AssetDatabase.LoadAssetAtPath<BuildingCard>("Assets/Scripts/Buildings/BuildingPlacemenntSystem/BuildingCard/WoodHut.asset");
                var go = UnityEngine.Object.Instantiate(card.completPrefab, new Vector3(400, 0, 400), Quaternion.identity);
                producer = go.GetComponent<HutStation>();
                typeof(HutStation).GetField("collectionInterval", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(producer, .2f);
                var node = new GameObject("Production test resource"); node.transform.position = go.transform.position + Vector3.right;
                resource = node.AddComponent<StaticResource>(); resource.Configure(ResourceType.Wood);
                wood = PlayerInfo.Instance.GetWood;
                since = EditorApplication.timeSinceStartup; step = 2; return;
            }
            if (step == 2 && EditorApplication.timeSinceStartup - since > 1)
            {
                Check(producer.IsActive() && !resource.IsActiveResource && PlayerInfo.Instance.GetWood > wood, "Supplied production must collect resources into the city stock.");
                producer.Deactivate();
                var node = new GameObject("Production disabled test resource"); node.transform.position = producer.transform.position + Vector3.right;
                resource = node.AddComponent<StaticResource>(); resource.Configure(ResourceType.Wood);
                wood = PlayerInfo.Instance.GetWood;
                since = EditorApplication.timeSinceStartup; step = 3; return;
            }
            if (step == 3 && EditorApplication.timeSinceStartup - since > .7)
            {
                Check(!producer.IsActive() && resource.IsActiveResource && PlayerInfo.Instance.GetWood == wood, "Disabled production must not collect resources.");
                producer.Activate();
                UnityEngine.Object.FindAnyObjectByType<DayNightCycle>().ToggleSpeed();
                since = EditorApplication.timeSinceStartup; step = 4; return;
            }
            if (step == 4 && EditorApplication.timeSinceStartup - since > 1)
            {
                Check(Time.timeScale == 2f && !resource.IsActiveResource && PlayerInfo.Instance.GetWood > wood, "Production must work while the simulation runs at 2x.");
                UnityEngine.Object.FindAnyObjectByType<DayNightCycle>().ToggleSpeed();
                SessionState.SetBool(Running, false); Debug.Log("CITY_MAP_REGRESSION_CHECK_PASSED"); EditorApplication.Exit(0);
            }
        }
        catch (Exception e) { InputSystem.onBeforeUpdate -= InjectMouse; if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse); SessionState.SetBool(Running, false); Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
