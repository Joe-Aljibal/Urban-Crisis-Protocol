using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Run with -executeMethod CitySceneSmokeCheck.Run (without -quit).
[InitializeOnLoad]
public static class CitySceneSmokeCheck
{
    private const string Running = "CitySceneSmokeCheck.Running";
    private static double started;
    private static int step;
    private static bool failed;

    static CitySceneSmokeCheck()
    {
        EditorApplication.update += Update;
        Application.logMessageReceived += OnLog;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Ara_Scripts/UiDesign/NewWorldDesign/NewWorldDesign.unity");
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        // Unity 6000.5's background search index can fail before play mode.
        // It is an editor-only error; gameplay exceptions must still fail the check.
        if (stack.Contains("UnityEditor.Search.SearchDatabase")) return;
        if (SessionState.GetBool(Running, false) &&
            (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)) failed = true;
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
                Require(CityStructure.All.Count == 6, "Six neighborhood structures must exist.");
                Require(UnityEngine.Object.FindAnyObjectByType<BuildingManager>() != null, "Original construction manager must exist.");
                Require(CityLegacyEvents.Instance != null && CityLegacyEvents.Instance.IsReady, "Legacy events must initialize.");
                Require(UnityEngine.Object.FindAnyObjectByType<DayNightCycle>() != null, "Day/night cycle must exist.");
                Require(UnityEngine.Object.FindObjectsByType<CityPedestrian>(FindObjectsSortMode.None).Length == 6, "Six pedestrians must exist.");
                CheckDisasterScenarios();
                CityLegacyEvents.Instance.StartRaid();
                Require(CityLegacyEvents.Instance.RaidRunning, "Raid must start.");
                step = 1;
            }
            if (step == 1 && elapsed > 6)
            {
                Require(CityLegacyEvents.Instance.DispatchSecurity(), "Security must stop an active raid.");
                Require(!CityLegacyEvents.Instance.RaidRunning, "Raid must stop.");
                CityLegacyEvents.Instance.LaunchBomb();
                step = 2;
            }
            if (elapsed > 20)
            {
                Require(CityStructure.All.Exists(building => building.IsDamaged), "Bomb must damage a city structure.");
                SessionState.SetBool(Running, false);
                Debug.Log(failed ? "CITY_SMOKE_CHECK_FAILED" : "CITY_SMOKE_CHECK_PASSED");
                EditorApplication.Exit(failed ? 1 : 0);
            }
        }
        catch (Exception exception)
        {
            SessionState.SetBool(Running, false);
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void CheckDisasterScenarios()
    {
        CityDisasterEvent disaster = CityDisasterEvent.Instance;
        CityStructure target = CityStructure.All[0];
        Vector3 location = target.transform.position;
        Require(disaster.TryStart(CityDisasterEvent.Disaster.Flood, location), "Flood warning must start.");
        Require(!disaster.TryStart(CityDisasterEvent.Disaster.Tornado, location), "An active incident must reject replacement.");
        disaster.Tick(11f);
        Require(disaster.TryGetDanger(location, out _), "Active flood must be dangerous.");
        disaster.Tick(26f);
        Require(target.IsDamaged && disaster.IsRoadBlocked(location), "Flood must damage structures and leave a blocked area.");
        Require(disaster.Repair(), "Flood recovery must succeed with sufficient funds.");
        Require(!target.IsDamaged && !disaster.IsRoadBlocked(location), "Recovery must clear damage and road closure.");

        Require(disaster.TryStart(CityDisasterEvent.Disaster.Thunderstorm, location), "Storm warning must start.");
        Require(disaster.DispatchResponse(), "Emergency preparation must succeed.");
        Require(!disaster.DispatchResponse(), "Preparation cannot charge twice.");
        disaster.Tick(11f);
        CityPowerOutage power = UnityEngine.Object.FindAnyObjectByType<CityPowerOutage>();
        Require(power.IsActive, "Thunderstorm must trigger a blackout.");
        Require(power.DispatchRepair(), "Blackout must be repairable.");
        disaster.Tick(26f);
        Require(disaster.Repair(), "Storm damage must be repairable.");

        Require(disaster.TryStart(CityDisasterEvent.Disaster.HeatWave, location), "Heat warning must start.");
        disaster.Tick(11f);
        disaster.Tick(26f);
        Require(!target.IsDamaged, "Heat must not directly damage building integrity.");
        CityPedestrian citizen = UnityEngine.Object.FindAnyObjectByType<CityPedestrian>();
        citizen.TakeExposure(80f);
        Require(citizen.IsInjured, "Severe exposure must injure a citizen.");
        Require(CityPedestrian.DispatchMedicalAid(), "Operational medical services must treat injured citizens.");
        Require(!citizen.IsInjured, "Medical aid must restore health.");
    }
}
