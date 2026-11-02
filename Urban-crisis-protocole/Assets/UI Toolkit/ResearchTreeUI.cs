using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchTreeUI : MonoBehaviour
{
    PanelRenderer panelRenderer;
    PlayerInfo playerInfo = PlayerInfo.Instance;
    VisualElement root;
    bool isResearchOpen = false;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(ResearchPageUI);
    }

    void ResearchPageUI(PanelRenderer renderer, VisualElement rootElement, int version)
    {
        root = rootElement;
        root.Q<Button>("close-button").clicked += () => ToggleResearchPage();

        Bind(playerInfo.housesUnlocked, "house-1", new[] { "house-2", "apartment-1" }, BuildingTypes.SmallHouse, 0);
        Bind(playerInfo.housesUnlocked, "house-2", new[] { "house-3" }, BuildingTypes.MediumHouse, 2);
        Bind(playerInfo.housesUnlocked, "house-3", null, BuildingTypes.LargeHouse, 5);
        Bind(playerInfo.housesUnlocked, "apartment-1", new[] { "apartment-2" }, BuildingTypes.SmallApartment, 0);
        Bind(playerInfo.housesUnlocked, "apartment-2", new[] { "apartment-3" }, BuildingTypes.MediumApartment, 2);
        Bind(playerInfo.housesUnlocked, "apartment-3", null, BuildingTypes.LargeApartment, 5);

        Bind(playerInfo.businessesUnlocked, "gas-station", new[] { "diner" }, BuildingTypes.GasStation, 0);
        Bind(playerInfo.businessesUnlocked, "diner", new[] { "fast-food" }, BuildingTypes.Diner, 2);
        Bind(playerInfo.businessesUnlocked, "fast-food", new[] { "office" }, BuildingTypes.FastFood, 5);
        Bind(playerInfo.businessesUnlocked, "office", new[] { "super-market" }, BuildingTypes.Office, 0);
        Bind(playerInfo.businessesUnlocked, "super-market", null, BuildingTypes.SuperMarket, 2);

        Bind(playerInfo.electricityUnlocked, "solar-panels", new[] { "wind-turbines" }, BuildingTypes.SolarPanels, 0);
        Bind(playerInfo.electricityUnlocked, "wind-turbines", new[] { "coal-power-station" }, BuildingTypes.WindTurbine, 2);
        Bind(playerInfo.electricityUnlocked, "coal-power-station", new[] { "solar-plant" }, BuildingTypes.CoalPowerStation, 5);
        Bind(playerInfo.electricityUnlocked, "solar-plant", new[] { "nuclear-plant" }, BuildingTypes.SolarPlant, 0);
        Bind(playerInfo.electricityUnlocked, "nuclear-plant", null, BuildingTypes.NuclearPlant, 2);

        Bind(playerInfo.waterUnlocked, "water-silo", new[] { "water-reservoir" }, BuildingTypes.WaterSilo, 0);
        Bind(playerInfo.waterUnlocked, "water-reservoir", new[] { "water-treatment-plant" }, BuildingTypes.WaterReservoir, 2);
        Bind(playerInfo.waterUnlocked, "water-treatment-plant", null, BuildingTypes.WaterTreatmentPlant, 5);
    }

    // Unlocking Logic
    void UnlockBuilding(string buttonName,  List<BuildingTypes> list, BuildingTypes type, int price, string[] nextButtons)
    {

        if (list.Contains(type)) return;

        if (CanUnlock(price))
        {
            VisualElement button = root.Q<Button>(buttonName);
            StartCoroutine(CreateBorder(button));

            list.Add(type);

            if (nextButtons != null)
            {
                foreach (var b in nextButtons)
                {
                    root.Q<Button>(b).SetEnabled(true);
                }
            }
        }
    }

    IEnumerator CreateBorder(VisualElement button)
    {
        button.style.borderRightColor = new StyleColor(Color.whiteSmoke);
        yield return new WaitForSeconds(.05f);
        button.style.borderBottomColor = new StyleColor(Color.whiteSmoke);
        yield return new WaitForSeconds(.05f);
        button.style.borderLeftColor = new StyleColor(Color.whiteSmoke);
        yield return new WaitForSeconds(.05f);
        button.style.borderTopColor = new StyleColor(Color.whiteSmoke);
        yield return new WaitForSeconds(.05f);
    }

    void Bind(List<BuildingTypes> list, string buttonName, string[] nextButtons, BuildingTypes type, int price)
    {
        Button btn = root.Q<Button>(buttonName);
        btn.clicked += () => UnlockBuilding(buttonName, list, type, price, nextButtons);

        if (list.Contains(type))
        {
            btn.style.borderRightColor = Color.whiteSmoke;
            btn.style.borderBottomColor = Color.whiteSmoke;
            btn.style.borderLeftColor = Color.whiteSmoke;
            btn.style.borderTopColor = Color.whiteSmoke;
            btn.SetEnabled(true);

            foreach (var b in nextButtons)
            {
                root.Q<Button>(b).SetEnabled(true);
            }
        }
    }

    bool CanUnlock(int price) => playerInfo.GetResearchPoints() >= price;



    public void ToggleResearchPage()
    {
        VisualElement treePage = root.Q("research-tree-root");

        if (isResearchOpen)
        {
            treePage.style.transitionDuration = new List<TimeValue> { new TimeValue(250, TimeUnit.Millisecond) };
            treePage.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
            treePage.style.translate = new Translate(0, 0);
            treePage.style.scale = new Scale(new Vector2(0, 0));

            isResearchOpen = false;
        }
        else
        {
            treePage.style.transitionDuration = new List<TimeValue> { new TimeValue(250, TimeUnit.Millisecond) };
            treePage.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
            treePage.style.translate = new Translate(1920, 0);
            treePage.style.scale = new Scale(new Vector2(1, 1));

            isResearchOpen = true;
        }
    }
}
