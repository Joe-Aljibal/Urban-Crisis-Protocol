using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CityWideEventsCheck
{
    const string Running = "CityWideEventsCheck.Running";
    static int step; static double since; static CityStructure west, near, east; static CityDisasterEvent disaster;
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static CityWideEventsCheck() { EditorApplication.update += Update; }
    public static void Run() => RunScene("Assets/Scenes/IntegratedCity.unity");
    public static void RunSource() => RunScene("Assets/Ara_Scripts/UiDesign/NewWorldDesign/NewWorldDesign.unity");
    private static void RunScene(string scene)
    {
        EditorSceneManager.OpenScene(scene);
        var tree = GameObject.Find("Tree_01");
        var populator = UnityEngine.Object.FindAnyObjectByType<NaturalResourcePopulator>();
        if (tree == null || populator == null) throw new Exception("Scene nature is missing.");
        float factor = new SerializedObject(populator).FindProperty("resourceVisualScale").floatValue;
        SessionState.SetFloat("CityWideEventsCheck.ExpectedTreeScale", tree.transform.localScale.x * factor);
        SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
    }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static CityStructure MakeBuilding(string name, Vector3 position)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = position;
        var building = obj.AddComponent<CityStructure>(); building.Configure("Construction", Color.white); return building;
    }
    static void Update()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
        if (since == 0) since = EditorApplication.timeSinceStartup;
        try
        {
            if (step == 0 && EditorApplication.timeSinceStartup - since > 4)
            {
                Check(GameObject.Find("Prototype neighborhood") == null, "The prototype neighbourhood must not be spawned.");
                var tree = GameObject.Find("Tree_01");
                Check(tree != null && Mathf.Abs(tree.transform.localScale.x - SessionState.GetFloat("CityWideEventsCheck.ExpectedTreeScale", -1f)) < .001f, "Authored trees must be scaled to half their original size.");
                west = MakeBuilding("West building", new Vector3(-150, 0, 60));
                near = MakeBuilding("Neighbour", new Vector3(-145, 0, 60));
                east = MakeBuilding("East building", new Vector3(150, 0, -60));
                var picked = new HashSet<CityStructure>();
                UnityEngine.Random.InitState(405);
                for (int i = 0; i < 100; i++) picked.Add(CityStructure.RandomIntact());
                Check(picked.Contains(west) && picked.Contains(east), "Random event selection must reach both sides of the city.");
                var legacy = CityLegacyEvents.Instance;
                Check(legacy != null && legacy.IsReady, "Raid and bomb must initialize without the prototype.");
                var manager = UnityEngine.Object.FindAnyObjectByType<BuildingManager>();
                var card = AssetDatabase.LoadAssetAtPath<BuildingCard>("Assets/Scripts/Buildings/BuildingPlacemenntSystem/BuildingCard/Houses/Small House.asset");
                manager.ChangeCurrentBuildingCard(card);
                Check(manager.PlaceSelectedInBuildArea(), "A real player building must be placeable.");
                var playerBuilding = UnityEngine.Object.FindAnyObjectByType<House>().GetComponent<CityStructure>();
                legacy.StartRaid();
                var raid = UnityEngine.Object.FindAnyObjectByType<BanditRaidEvent>();
                var targets = (BanditRaidEvent.MoneyBuilding[])typeof(BanditRaidEvent).GetField("moneyBuildings", Flags).GetValue(raid);
                Check(targets.Length == 4 && Array.Exists(targets, t => t.building == west.transform) && Array.Exists(targets, t => t.building == east.transform) && Array.Exists(targets, t => t.building == playerBuilding.transform), "Raid targets must include real buildings across the city.");
                for (int i = 0; i < raid.BanditCount; i++) Check(legacy.TryGetDanger(targets[i].building.position, out _), "Raid danger must cover each attacked building.");
                Check(legacy.DispatchSecurity(), "Security must be able to stop the raid.");
                playerBuilding.DestroyObject();
                UnityEngine.Object.Destroy(playerBuilding.gameObject);
                legacy.LaunchBomb();
                var drop = (Transform)typeof(CityLegacyEvents).GetField("bombDropPoint", Flags).GetValue(legacy);
                Vector3 point = drop.position - Vector3.up * 180f;
                Check(Vector3.Distance(point, west.transform.position) < .01f || Vector3.Distance(point, near.transform.position) < .01f || Vector3.Distance(point, east.transform.position) < .01f, "The bomb must target a city building.");
                var fallingBomb = UnityEngine.Object.FindAnyObjectByType<BombFallController>();
                Check(fallingBomb != null, "The bomb must launch without a prototype target.");
                UnityEngine.Object.Destroy(fallingBomb.gameObject);
                var fire = CityFireEvent.Instance; fire.StartFireEvent();
                var affected = (CityStructure)typeof(CityFireEvent).GetField("affectedBuilding", Flags).GetValue(fire);
                Check(affected != null, "Fire must choose a city building.");
                typeof(CityFireEvent).GetMethod("StartFire", Flags).Invoke(fire, null);
                var damaged = (List<CityStructure>)typeof(CityFireEvent).GetField("damagedBuildings", Flags).GetValue(fire);
                Check(damaged.Count == 1 && damaged[0] == affected, "Fire must not automatically damage a distant district.");
                typeof(CityFireEvent).GetField("timer", Flags).SetValue(fire, 19f);
                typeof(CityFireEvent).GetMethod("UpdateBuildingDamage", Flags).Invoke(fire, null);
                foreach (var building in damaged) Check(Vector3.Distance(building.transform.position, affected.transform.position) <= 11.01f, "Fire must spread only to nearby buildings.");
                typeof(CityFireEvent).GetMethod("FinishFire", Flags).Invoke(fire, new object[] { true });
                disaster = CityDisasterEvent.Instance;
                Check(disaster.TryStart(CityDisasterEvent.Disaster.Tornado, west.transform.position), "Tornado must start at the chosen location.");
                disaster.Tick(11f);
                Check(Mathf.Abs(Vector3.Distance(disaster.Location, west.transform.position) - 25f) < .1f, "Tornado must start near its selected location.");
                disaster.Tick(12f);
                Check(west.IsDestroyed && near.IsDestroyed && !east.IsDestroyed, "Tornado must sweep buildings along its path, not a fixed prototype district.");
                disaster.Tick(1f); Check(disaster.NeedsRepair && disaster.Repair(), "Tornado damage must remain repairable.");
                Check(disaster.TryStart(CityDisasterEvent.Disaster.Earthquake, east.transform.position), "Earthquake must start at the east side.");
                disaster.Tick(11f); disaster.Tick(13f);
                Check(east.IsDestroyed && !west.IsDestroyed, "Earthquake damage must be local to the selected area.");
                disaster.Tick(13f); Check(disaster.Repair(), "Earthquake damage must remain repairable.");
                Check(disaster.TryStart(CityDisasterEvent.Disaster.Thunderstorm, east.transform.position), "Thunderstorm must start.");
                disaster.Tick(11f); since = EditorApplication.timeSinceStartup; step = 1; return;
            }
            if (step == 1 && EditorApplication.timeSinceStartup - since > 6)
            {
                Check(disaster.LightningStrikeCount == 3 && west.IsDestroyed && near.IsDestroyed && east.IsDestroyed, "Lightning must select from all intact city buildings.");
                disaster.Tick(26f); Check(disaster.Repair(), "Thunderstorm damage must remain repairable.");
                Check(UnityEngine.Object.FindAnyObjectByType<CityPowerOutage>().DispatchRepair(), "Power must be repairable after the storm.");
                SessionState.SetBool(Running, false); Debug.Log("CITY_WIDE_EVENTS_CHECK_PASSED"); EditorApplication.Exit(0);
            }
        }
        catch (Exception error) { SessionState.SetBool(Running, false); Debug.LogException(error); EditorApplication.Exit(1); }
    }
}
