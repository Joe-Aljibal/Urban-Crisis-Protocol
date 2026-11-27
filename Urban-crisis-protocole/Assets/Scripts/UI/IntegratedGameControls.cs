using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Provides access to city-wide systems from the integrated world.
public class IntegratedGameControls : MonoBehaviour
{
    private bool researchOpen;
    private Vector2 scroll;
    private GambleLogic casino;
    private static IntegratedGameControls instance;

    private void Awake() { instance = this; casino = gameObject.AddComponent<GambleLogic>(); }
    private void OnDestroy() { if (instance == this) instance = null; }

    public void OpenCasino() => casino.OpenPanel();

    private static bool CasinoVisible => instance != null && FindAnyObjectByType<JackCityUI>() == null && !CityConstruction.IsMenuOpen && CityFireEvent.DashboardBottom + 48f <= Screen.height;
    private static Rect CasinoRect => new Rect(CityFireEvent.TabRect.x, CityFireEvent.DashboardBottom + 8f, CityFireEvent.TabRect.width, 40f);
    public static bool IsPointerOverCasino => CasinoVisible && Mouse.current != null &&
        CasinoRect.Contains(new Vector2(Mouse.current.position.ReadValue().x, Screen.height - Mouse.current.position.ReadValue().y));

    private void OnGUI()
    {
        if (CasinoVisible && GUI.Button(CasinoRect, "Casino")) casino.OpenPanel();
        if (FindAnyObjectByType<JackCityUI>() == null && GUI.Button(new Rect(Screen.width - 220, 20, 200, 40), researchOpen ? "Fermer recherche" : "Recherche")) researchOpen = !researchOpen;
        if (!researchOpen) return;

        var player = PlayerInfo.Instance;
        GUILayout.BeginArea(new Rect(Screen.width - 350, 130, 330, Mathf.Min(500, Screen.height - 150)), GUI.skin.box);
        GUILayout.Label("Points de recherche : " + player.GetResearchPoints().ToString("F1"));
        scroll = GUILayout.BeginScrollView(scroll);
        Unlock(player.housesUnlocked, BuildingTypes.SmallHouse, BuildingTypes.MediumHouse, 3);
        Unlock(player.housesUnlocked, BuildingTypes.MediumHouse, BuildingTypes.LargeHouse, 12);
        Unlock(player.housesUnlocked, BuildingTypes.SmallHouse, BuildingTypes.SmallApartment, 3);
        Unlock(player.housesUnlocked, BuildingTypes.SmallApartment, BuildingTypes.MediumApartment, 7);
        Unlock(player.housesUnlocked, BuildingTypes.MediumApartment, BuildingTypes.LargeApartment, 12);
        Unlock(player.businessesUnlocked, BuildingTypes.GasStation, BuildingTypes.Diner, 3);
        Unlock(player.businessesUnlocked, BuildingTypes.Diner, BuildingTypes.FastFood, 6);
        Unlock(player.businessesUnlocked, BuildingTypes.FastFood, BuildingTypes.Office, 12);
        Unlock(player.electricityUnlocked, BuildingTypes.SolarPanels, BuildingTypes.WindTurbine, 3);
        Unlock(player.electricityUnlocked, BuildingTypes.WindTurbine, BuildingTypes.CoalPowerStation, 8);
        Unlock(player.electricityUnlocked, BuildingTypes.CoalPowerStation, BuildingTypes.SolarPlant, 12);
        Unlock(player.electricityUnlocked, BuildingTypes.SolarPlant, BuildingTypes.NuclearPlant, 20);
        Unlock(player.waterUnlocked, BuildingTypes.WaterSilo, BuildingTypes.WaterReservoir, 4);
        Unlock(player.waterUnlocked, BuildingTypes.WaterReservoir, BuildingTypes.WaterTreatmentPlant, 15);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static void Unlock(List<BuildingTypes> unlocked, BuildingTypes prerequisite, BuildingTypes next, int cost)
    {
        if (unlocked.Contains(next)) { GUILayout.Label(next + " : debloque"); return; }
        GUI.enabled = unlocked.Contains(prerequisite) && PlayerInfo.Instance.GetResearchPoints() >= cost;
        if (GUILayout.Button(next + " : " + cost + " points"))
        {
            PlayerInfo.Instance.AddResearchPoints(-cost);
            unlocked.Add(next);
        }
        GUI.enabled = true;
    }
}
