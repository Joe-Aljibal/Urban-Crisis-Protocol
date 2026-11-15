using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CitySceneSmokeCheck
{
    private const string Running = "CitySceneSmokeCheck.Running";
    private static double started;
    private static int step;
    private static bool failed;
    private static CityDisasterEvent disaster;
    private static CityStructure city;
    private static CityStructure built;

    static CitySceneSmokeCheck()
    {
        EditorApplication.update += Update;
        Application.logMessageReceived += (message, stack, type) =>
        {
            if (stack.Contains("UnityEditor.Search.SearchDatabase")) return;
            if (SessionState.GetBool(Running, false) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) failed = true;
        };
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Ara_Scripts/UiDesign/NewWorldDesign/NewWorldDesign.unity");
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Update()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
        if (started == 0) started = EditorApplication.timeSinceStartup;
        double elapsed = EditorApplication.timeSinceStartup - started;
        try
        {
            if (step == 0 && elapsed > 3)
            {
                BuildingManager manager = UnityEngine.Object.FindAnyObjectByType<BuildingManager>();
                CityConstruction construction = UnityEngine.Object.FindAnyObjectByType<CityConstruction>();
                Check(manager != null && construction != null, "Construction must initialize.");

                construction.ToggleMenu();
                Check(CityConstruction.IsMenuOpen, "Construction must open.");
                construction.SelectNextBuilding();
                Check(manager.HasSelection, "Construction must select a building.");
                var preview = (GameObject)typeof(BuildingManager).GetField("currentSelectedBuildingClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(manager);
                Quaternion beforeRotation = preview.transform.rotation;
                manager.RotatePreview();
                Check(Quaternion.Angle(beforeRotation, preview.transform.rotation) > 80f, "Construction must rotate the preview.");
                manager.ResetDefaultBuilding();
                construction.ToggleMenu();
                Check(!CityConstruction.IsMenuOpen, "Construction must close.");
                BuildingCard house = AssetDatabase.LoadAssetAtPath<BuildingCard>("Assets/Scripts/Buildings/BuildingPlacemenntSystem/BuildingCard/House.asset");
                Check(manager.TryPlace(house, new Vector3(-120f, 0.16f, -55f), Quaternion.identity), "House must be built.");
                city = CityStructure.All.Find(building => building != null && building.District == "Residential");
                built = CityStructure.All.Find(building => building != null && building.District == "Construction");
                Check(city != null && built != null, "Both areas must have damageable buildings.");

                Check(Array.IndexOf(Enum.GetNames(typeof(CityDisasterEvent.Disaster)), "Flood") < 0, "Flood must not be selectable.");
                CityFireEvent fire = CityFireEvent.Instance;
                fire.StartFireEvent();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                typeof(CityFireEvent).GetMethod("StartFire", flags).Invoke(fire, null);
                var fireTargets = (System.Collections.Generic.List<CityStructure>)typeof(CityFireEvent).GetField("damagedBuildings", flags).GetValue(fire);
                Check(fireTargets.Exists(item => item.District == "Construction") && fireTargets.Exists(item => item.District != "Construction"), "Fire must affect both areas.");
                typeof(CityFireEvent).GetMethod("FinishFire", flags).Invoke(fire, new object[] { true });
                CityLegacyEvents.Instance.StartRaid();
                BanditRaidEvent raid = UnityEngine.Object.FindAnyObjectByType<BanditRaidEvent>();
                var raidTargets = (BanditRaidEvent.MoneyBuilding[])typeof(BanditRaidEvent).GetField("moneyBuildings", flags).GetValue(raid);
                Check(raidTargets.Length == 2, "Bandit raids must target both areas.");
                Check(CityLegacyEvents.Instance.DispatchSecurity(), "Raid must be stoppable.");

                disaster = CityDisasterEvent.Instance;
                Check(disaster.TryStart(CityDisasterEvent.Disaster.Tornado, city.transform.position), "Tornado must start.");
                disaster.Tick(11f);
                GameObject effect = (GameObject)typeof(CityDisasterEvent).GetField("effect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(disaster);
                Check(effect != null && effect.name.StartsWith("Tornado"), "The original tornado prefab must be used.");
                Vector3 firstPosition = effect.transform.position;
                disaster.Tick(1f);
                Check(Mathf.Abs(Vector3.Distance(firstPosition, effect.transform.position) - 8f) < 0.1f, "Tornado must move at its original speed.");
                for (int i = 0; i < 15 && !disaster.NeedsRepair; i++) disaster.Tick(1f);
                Check(CityStructure.All.Exists(item => item.District != "Construction" && item.IsDestroyed), "Tornado must destroy a city building.");
                Check(built.IsDestroyed, "Tornado must reach the construction area.");
                Check(disaster.Repair(), "Tornado damage must be repairable.");

                Check(disaster.TryStart(CityDisasterEvent.Disaster.Earthquake, city.transform.position), "Earthquake must start.");
                disaster.Tick(11f);
                disaster.Tick(26f);
                Check(city.IsDestroyed && built.IsDestroyed, "Earthquake must destroy buildings in both areas.");
                Check(disaster.Repair(), "Earthquake damage must be repairable.");

                Check(disaster.TryStart(CityDisasterEvent.Disaster.Thunderstorm, city.transform.position), "Thunderstorm must start.");
                disaster.Tick(11f);
                Check(disaster.LightningStrikeCount == 1, "The first strike must start immediately.");
                Check(!built.GetComponent<House>().enabled, "An outage must pause constructed building functions.");
                step = 1;
            }
            if (step == 1 && elapsed > 9)
            {
                Check(disaster.LightningStrikeCount == 3, "Thunderstorm must create exactly three strikes.");
                Check(CityStructure.All.FindAll(item => item.District != "Construction" && item.IsDestroyed).Count == 2, "Two lightning strikes must hit city buildings.");
                Check(built.IsDestroyed, "One lightning strike must hit a constructed building.");
                disaster.Tick(26f);
                Check(disaster.Repair(), "Lightning damage must be repairable.");
                Check(!built.IsDestroyed && !city.IsDestroyed, "Repair must restore buildings.");
                Check(UnityEngine.Object.FindAnyObjectByType<CityPowerOutage>().DispatchRepair(), "Power repair must work.");
                Check(built.GetComponent<House>().enabled, "Power repair must resume constructed building functions.");
                Check(!failed, "The scene must not produce gameplay errors.");
                SessionState.SetBool(Running, false);
                Debug.Log("CITY_SCENE_CHECK_PASSED");
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            SessionState.SetBool(Running, false);
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }
}
