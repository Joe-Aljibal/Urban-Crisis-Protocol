using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class CityPlacementCheck
{
    const string Running = "CityPlacementCheck.Running";
    static double started;
    static CityPlacementCheck() { EditorApplication.update += Update; }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/IntegratedCity.unity");
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Update()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
        if (started == 0) started = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - started < 5) return;
        try
        {
            var construction = UnityEngine.Object.FindAnyObjectByType<CityConstruction>();
            var manager = UnityEngine.Object.FindAnyObjectByType<BuildingManager>();
            Check(construction != null && manager != null, "Construction must initialize.");
            var cards = construction.Cards.Where(c => c != null && c.completPrefab != null && c.completPrefab.GetComponent<IBuilding>() != null && c.completPrefab.GetComponent<IBuilding>().GetBuildingSO() != null).ToArray();
            string[] names = { "Fast Food Card", "Coal Power Plant Card", "MilitaryBaseCard", "Large House", "Large Apartment", "Super Market Card" };
            foreach (string name in names) Check(cards.Any(c => c.name == name), "Missing card: " + name);
            PlayerInfo.Instance.AddMoney(1000000);
            foreach (var card in cards)
            {
                float money = PlayerInfo.Instance.GetMoney;
                construction.SelectCard(card);
                Check(manager.HasSelection, "Selection failed: " + card.name);
                Check(PlayerInfo.Instance.GetMoney == money, "Preview changed money: " + card.name);
                Check(manager.PlaceSelectedInBuildArea(), "Placement failed: " + card.name + " - " + manager.Status);
                Check(PlayerInfo.Instance.GetMoney == money - card.completPrefab.GetComponent<IBuilding>().GetPrice(), "Incorrect charge: " + card.name);
            }
            var ui = UnityEngine.Object.FindAnyObjectByType<JackCityUI>();
            Check(ui != null, "Building interface missing.");
            var root = (VisualElement)typeof(JackCityUI).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui);
            Check(root != null, "Building interface did not initialize.");
            PlayerInfo.Instance.housesUnlocked.Add(BuildingTypes.LargeHouse);
            PlayerInfo.Instance.housesUnlocked.Add(BuildingTypes.LargeApartment);
            PlayerInfo.Instance.businessesUnlocked.Add(BuildingTypes.FastFood);
            PlayerInfo.Instance.businessesUnlocked.Add(BuildingTypes.SuperMarket);
            PlayerInfo.Instance.electricityUnlocked.Add(BuildingTypes.CoalPowerStation);
            var show = typeof(JackCityUI).GetMethod("ShowSection", BindingFlags.NonPublic | BindingFlags.Instance);
            string[][] expected = { new[] { "Large House", "Large Apartment" }, new[] { "Fast Food", "Super Market" }, new[] { "Coal Power Station" }, new[] { "Military Base" } };
            int[] categories = { 0, 1, 2, 4 };
            for (int i = 0; i < categories.Length; i++)
            {
                show.Invoke(ui, new object[] { categories[i] });
                var buttons = root.Q<ScrollView>("BuildingsContainer").contentContainer.Children().OfType<UnityEngine.UIElements.Button>().ToArray();
                foreach (string name in expected[i]) Check(buttons.Any(b => b.tooltip.Contains(name)), "Unlocked building not visible: " + name);
            }
            Check(root.Query<UnityEngine.UIElements.Button>().ToList().Any(b => b.text == "Place automatically"), "Automatic placement button missing.");
            foreach (string name in new[] { "CasinoButton", "EventsButton", "AtomicBombButton" })
            {
                var action = root.Q<UnityEngine.UIElements.Button>(name);
                Check(action != null && action.style.backgroundImage.value.texture != null, "Toolbar icon missing: " + name);
            }
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/Building Menu.uxml");
            Check(asset != null, "Toolbar layout missing.");
            string markup = System.IO.File.ReadAllText("Assets/UI Toolkit/Building Menu.uxml");
            foreach (string name in new[] { "Casino", "Events", "AtomicBomb" })
                Check(markup.Contains("ToolbarIcons/" + name + ".png") && AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/ToolbarIcons/" + name + ".png") != null, "Editor icon reference missing: " + name);
            var showActions = typeof(JackCityUI).GetMethod("ShowActionSection", BindingFlags.NonPublic | BindingFlags.Instance);
            showActions.Invoke(ui, new object[] { 7 });
            var menu = root.Q<ScrollView>("BuildingsContainer");
            var actions = menu.contentContainer.Children().OfType<UnityEngine.UIElements.Button>().Select(b => b.text).ToArray();
            foreach (string name in new[] { "Fire", "Blackout", "Raid", "Tornado", "Earthquake", "Storm", "Manage" })
                Check(actions.Contains(name), "Missing event action: " + name);
            showActions.Invoke(ui, new object[] { 7 });
            Check(menu.style.display.value == DisplayStyle.None, "Event menu must toggle closed.");
            showActions.Invoke(ui, new object[] { 6 });
            Check(menu.contentContainer.Children().OfType<UnityEngine.UIElements.Button>().Any(b => b.text == "Open casino"), "Casino submenu missing.");
            showActions.Invoke(ui, new object[] { 8 });
            Check(menu.contentContainer.Children().OfType<UnityEngine.UIElements.Button>().Any(b => b.text == "Launch bomb"), "Bomb submenu missing.");
            var clock = UnityEngine.Object.FindAnyObjectByType<DayNightCycle>();
            Check(clock != null, "Day/night cycle missing.");
            Time.timeScale = 1f; clock.SetFastForward(true); Check(Time.timeScale == 2f, "Space must speed the simulation to 2x.");
            clock.SetFastForward(false); Check(Time.timeScale == 1f, "Releasing space must restore normal speed.");
            Time.timeScale = 0f; clock.SetFastForward(true); Check(Time.timeScale == 0f, "Fast-forward must not unpause the game.");
            Time.timeScale = 1f;
            var fire = CityFireEvent.Instance; Check(fire != null, "Events dashboard missing.");
            bool open = CityFireEvent.IsMenuOpen; fire.TogglePanel(); Check(CityFireEvent.IsMenuOpen != open, "Events action must toggle the dashboard."); fire.TogglePanel();
            var legacy = CityLegacyEvents.Instance; Check(legacy != null && legacy.IsReady, "Atomic bomb event must be ready.");
            legacy.LaunchBomb(); Check(UnityEngine.Object.FindAnyObjectByType<BombFallController>() != null, "Atomic bomb action must launch a bomb.");
            var game = new GameObject("Casino test");
            var logic = game.AddComponent<GambleLogic>(); logic.OpenPanel();
            var panel = UnityEngine.Object.FindAnyObjectByType<GamblingPanelUI>();
            Check(panel != null, "Casino panel missing.");
            var button = panel.GetComponentsInChildren<UnityEngine.UI.Button>(true).FirstOrDefault(b => b.name == "Close Casino Button");
            Check(button != null, "Casino close button missing."); button.onClick.Invoke();
            Check(!panel.gameObject.activeSelf, "Casino did not close."); logic.OpenPanel(); Check(panel.gameObject.activeSelf, "Casino did not reopen.");
            SessionState.SetBool(Running, false); Debug.Log("CITY_PLACEMENT_CHECK_PASSED"); EditorApplication.Exit(0);
        }
        catch (Exception e) { SessionState.SetBool(Running, false); Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
