using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[InitializeOnLoad]
public static class CityIncomeSpeedCheck
{
    const string Running = "CityIncomeSpeedCheck.Running";
    static int step; static double since; static float funds, scaledStart; static Keyboard keyboard; static Business station; static GambleBuilding casino; static bool injectSpace; static bool inputCastError; static Mouse mouse;
    static void InjectInput() { if (keyboard != null && keyboard.added) InputSystem.QueueStateEvent(keyboard, injectSpace ? new KeyboardState(Key.Space) : new KeyboardState()); }
    static CityIncomeSpeedCheck() { EditorApplication.update += Update; Application.logMessageReceived += CaptureErrors; }
    static void CaptureErrors(string message, string stack, LogType type)
    {
        if (SessionState.GetBool(Running, false) && type == LogType.Exception && message.Contains("InvalidCastException") && stack.Contains("DayNightCycle")) inputCastError = true;
    }
    public static void Run() { EditorSceneManager.OpenScene("Assets/Scenes/IntegratedCity.unity"); SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode(); }
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Update()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
        if (since == 0) since = EditorApplication.timeSinceStartup;
        var elapsed = EditorApplication.timeSinceStartup - since;
        try
        {
            if (step == 0 && elapsed > 4)
            {
                mouse = InputSystem.AddDevice<Mouse>();
                foreach (var collector in UnityEngine.Object.FindObjectsByType<SimpleResourceCollector>()) collector.enabled = false;
                foreach (var collector in UnityEngine.Object.FindObjectsByType<ResourceWorker>()) collector.enabled = false;
                var manager = UnityEngine.Object.FindAnyObjectByType<BuildingManager>();
                Check(manager != null, "Construction manager missing");
                PlayerInfo.Instance.AddMoney(10000);
                string basePath = "Assets/Scripts/Buildings/BuildingPlacemenntSystem/BuildingCard/";
                string[] paths = { "Utilities/Electricity/Solar Panel Card.asset", "Utilities/Electricity/Solar Panel Card.asset", "Utilities/Water/Water Silo Card.asset", "Houses/Small House.asset", "Businesses/Gas Station Card.asset" };
                for (int i = 0; i < paths.Length; i++)
                {
                    var card = AssetDatabase.LoadAssetAtPath<BuildingCard>(basePath + paths[i]);
                    Check(card != null, "Missing card " + paths[i]);
                    manager.ChangeCurrentBuildingCard(card);
                    Check(manager.PlaceSelectedInBuildArea(), "Cannot place " + paths[i] + ": " + manager.Status);
                }
                step = 1; since = EditorApplication.timeSinceStartup; return;
            }
            if (step == 1 && elapsed > 1)
            {
                station = UnityEngine.Object.FindAnyObjectByType<Business>();
                Check(station != null && station.IsActive(), "Gas station must be supplied with power and workers");
                var ui = UnityEngine.Object.FindAnyObjectByType<JackCityUI>();
                var root = (UnityEngine.UIElements.VisualElement)typeof(JackCityUI).GetField("root", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(ui);
                Check(root != null, "Dashboard did not initialize");
                var population = UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Label>(root, "PopulationValue");
                Check(population.text == PlayerInfo.Instance.GetAvailablePopulation + " / " + PlayerInfo.Instance.GetPopulation, "Population icon must show the same available/total values as the HUD");
                Check(UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Button>(root, "SpeedButton") != null, "Speed control must be visible on the dashboard");
                typeof(JackCityUI).GetField("selected", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(ui, station);
                typeof(JackCityUI).GetField("selectedObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(ui, station.gameObject);
                var updateStatus = typeof(JackCityUI).GetMethod("UpdateSelectedBuilding", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                updateStatus.Invoke(ui, null);
                var status = UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Label>(root, "BuildingStatus");
                Check(status.text.Contains("+$75"), "Operating business must show its income");
                station.Deactivate(0, 0); updateStatus.Invoke(ui, null); Check(status.text.Contains("Needs:"), "Inactive business must explain missing supplies"); station.Activate();
                funds = PlayerInfo.Instance.GetMoney; since = EditorApplication.timeSinceStartup; step = 2; return;
            }
            if (step == 2 && elapsed > 2.2)
            {
                Check(PlayerInfo.Instance.GetMoney >= funds + 75, "A supplied business must generate actual income over time");
                station.enabled = false;
                var config = ScriptableObject.CreateInstance<GambleBuildingSO>(); config.profit = 10;
                casino = new GameObject("Income test casino").AddComponent<GambleBuilding>();
                typeof(GambleBuilding).GetField("gambleBuildingSO", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(casino, config);
                funds = PlayerInfo.Instance.GetMoney; since = EditorApplication.timeSinceStartup; step = 21; return;
            }
            if (step == 21 && elapsed > 1.5)
            {
                Check(casino.IsActive() && PlayerInfo.Instance.GetMoney >= funds + 10, "An operating casino building must generate income");
                station.enabled = true;
                keyboard = InputSystem.AddDevice<Keyboard>(); injectSpace = true; InputSystem.onBeforeUpdate += InjectInput;
                since = EditorApplication.timeSinceStartup; scaledStart = Time.time; step = 3; return;
            }
            if (step == 3 && elapsed > 1.2)
            {
                var clock = UnityEngine.Object.FindAnyObjectByType<DayNightCycle>();
                Debug.Log("SPEED_DIAGNOSTIC scale=" + Time.timeScale + " keyboardPressed=" + keyboard.spaceKey.isPressed + " clock=" + (clock != null ? clock.name + " enabled=" + clock.enabled + " active=" + clock.gameObject.activeInHierarchy : "missing") + " observedSpace=" + (clock != null && clock.SpaceHeld) + " debugScripts=" + UnityEngine.Object.FindObjectsByType<DebugHutStationPlacement>().Length);
                Check(Mathf.Approximately(Time.timeScale, 2), "Holding Space must actually activate 2x speed");
                Check(Time.time - scaledStart > 1.6f, "Simulation time must advance faster while Space is held");
                injectSpace = false; since = EditorApplication.timeSinceStartup; step = 4; return;
            }
            if (step == 4 && elapsed > .3)
            {
                Check(Mathf.Approximately(Time.timeScale, 1), "Releasing Space must restore normal speed");
                InputSystem.onBeforeUpdate -= InjectInput; InputSystem.RemoveDevice(keyboard); keyboard = null;
                UnityEngine.Object.FindAnyObjectByType<DayNightCycle>().ToggleSpeed(); since = EditorApplication.timeSinceStartup; step = 5; return;
            }
            if (step == 5 && elapsed > .3)
            {
                Check(Mathf.Approximately(Time.timeScale, 2), "The speed button must enable 2x");
                UnityEngine.Object.FindAnyObjectByType<DayNightCycle>().ToggleSpeed(); since = EditorApplication.timeSinceStartup; step = 6; return;
            }
            if (step == 6 && elapsed > .3)
            {
                Check(Mathf.Approximately(Time.timeScale, 1), "The speed button must restore 1x");
                Time.timeScale = 0; since = EditorApplication.timeSinceStartup; step = 7; return;
            }
            if (step == 7 && elapsed > .3)
            {
                Check(Time.timeScale == 0, "The speed control must not override pause");
                Check(!inputCastError, "Non-keyboard input devices must not cause an InvalidCastException");
                if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse); Time.timeScale = 1;
                SessionState.SetBool(Running, false); Debug.Log("CITY_INCOME_SPEED_CHECK_PASSED"); EditorApplication.Exit(0);
            }
        }
        catch (Exception e) { if (keyboard != null && keyboard.added) InputSystem.onBeforeUpdate -= InjectInput; InputSystem.RemoveDevice(keyboard); SessionState.SetBool(Running, false); Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
