using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Connects Jack's panel to the integrated city's existing systems.
public class JackCityUI : MonoBehaviour
{
    private PanelRenderer panel;
    private VisualElement root;
    private CityConstruction construction;
    private ResearchTreeUI research;
    private IBuilding selected;
    private GameObject selectedObject;
    private Label placementStatus;
    private bool showCards;
    private int section = -1;

    private void Awake()
    {
        panel = GetComponent<PanelRenderer>();
        research = GetComponent<ResearchTreeUI>();
        panel.RegisterUIReloadCallback(Setup);
    }

    private void Start() => construction = FindAnyObjectByType<CityConstruction>();
    private void OnDestroy() { if (panel != null) panel.UnregisterUIReloadCallback(Setup); }

    private void Setup(PanelRenderer renderer, VisualElement visual, int version)
    {
        root = visual;
        placementStatus = new Label();
        placementStatus.style.position = Position.Absolute;
        placementStatus.style.top = 120;
        placementStatus.style.left = 20;
        placementStatus.style.color = Color.white;
        placementStatus.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 0.9f);
        placementStatus.style.paddingLeft = 12;
        placementStatus.style.paddingRight = 12;
        placementStatus.style.paddingTop = 6;
        placementStatus.style.paddingBottom = 6;
        placementStatus.style.display = DisplayStyle.None;
        root.Add(placementStatus);
        root.Q("BuildingInfo").style.display = DisplayStyle.None;
        root.Q("BuildingPopUp").style.display = DisplayStyle.None;
        root.Q("BuildingsContainer").style.display = DisplayStyle.None;
        string[] buttons = { "HousesSelector", "BusinessesSelector", "ElectricitySelector", "WaterSelector", "MilitarySelector", "ResearchSelector" };
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            root.Q<Button>(buttons[i]).clicked += () => ShowSection(index);
        }
        root.Q<Button>("ResearchTreeButton").clicked += () => research.ToggleResearchPage();
        root.Q<Button>("SellButton").clicked += SellSelected;
    }

    private void Update()
    {
        if (root == null) return;
        var p = PlayerInfo.Instance;
        if (construction == null) construction = FindAnyObjectByType<CityConstruction>();
        var manager = construction != null ? construction.GetComponentInChildren<BuildingManager>() : null;
        if (manager == null) manager = FindAnyObjectByType<BuildingManager>();
        if (placementStatus != null && manager != null)
        {
            placementStatus.text = manager.Status;
            placementStatus.style.display = manager.HasSelection ? DisplayStyle.Flex : DisplayStyle.None;
        }
        Set("MoneyValue", "$" + p.GetMoney.ToString("F0"));
        Set("PopulationValue", p.GetWorkingPopulation + " / " + p.GetPopulation);
        Set("ElectricityValue", p.GetElectricityUsed + " / " + p.GetElectricity);
        Set("WaterValue", p.GetWaterUsed + " / " + p.GetWater);
        Set("ResearchPointsValue", p.GetResearchPoints().ToString("F0"));
        Set("ResearchPoints", p.GetResearchPoints().ToString("F0"));
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || Camera.main == null) return;
        var pos = Mouse.current.position.ReadValue();
        if (pos.y < 160 || (selectedObject != null && pos.x > Screen.width - 300 && pos.y > Screen.height - 420)) return;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out RaycastHit hit))
        {
            var building = hit.collider.GetComponentInParent<MonoBehaviour>();
            if (building == null) return;
            var target = hit.collider.GetComponentInParent<CityStructure>();
            if (target == null) return;
            selected = target.GetComponent<IBuilding>();
            selectedObject = selected != null ? target.gameObject : null;
            if (selected != null) ShowBuilding();
        }
    }

    private void Set(string name, string value) { var label = root.Q<Label>(name); if (label != null) label.text = value; }

    private void ShowSection(int value)
    {
        if (construction == null) construction = FindAnyObjectByType<CityConstruction>();
        if (construction == null) return;
        showCards = !showCards || section != value;
        section = value;
        var container = root.Q<ScrollView>("BuildingsContainer");
        container.Clear();
        container.style.translate = new Translate(0, -120);
        container.style.display = showCards ? DisplayStyle.Flex : DisplayStyle.None;
        if (!showCards) return;
        foreach (var card in construction.Cards)
        {
            if (card == null || card.completPrefab == null) continue;
            var building = card.completPrefab.GetComponent<IBuilding>();
            if (building == null) continue;
            IBuildingSO so = building.GetBuildingSO();
            if (so == null || Category(so) != section || !IsUnlocked(so)) continue;
            var button = new Button(() => { construction.SelectCard(card); container.style.display = DisplayStyle.None; showCards = false; }) { text = so.GetIcon() == null ? card.name : "" };
            if (so.GetIcon() != null) button.style.backgroundImage = new StyleBackground(so.GetIcon());
            button.style.width = 80; button.style.height = 80;
            button.tooltip = so.GetBuildingName();
            button.RegisterCallback<MouseEnterEvent>(_ => ShowDescription(so));
            button.RegisterCallback<MouseLeaveEvent>(_ => root.Q("BuildingInfo").style.display = DisplayStyle.None);
            container.Add(button);
        }
    }

    private static int Category(IBuildingSO so)
    {
        if (so is HousesSO) return 0;
        if (so is BusinessesSO) return 1;
        if (so is ElectricitySO) return 2;
        if (so is WaterSO) return 3;
        if (so is MilitaryBaseSO) return 4;
        return 5;
    }
    private static bool IsUnlocked(IBuildingSO so)
    {
        var p = PlayerInfo.Instance;
        List<BuildingTypes> list = Category(so) switch { 0 => p.housesUnlocked, 1 => p.businessesUnlocked, 2 => p.electricityUnlocked, 3 => p.waterUnlocked, 4 => p.militaryUnlocked, _ => null };
        return list == null || list.Contains(so.GetBuildingType());
    }
    private void ShowDescription(IBuildingSO so)
    {
        var info = root.Q("BuildingInfo");
        info.style.display = DisplayStyle.Flex;
        info.style.translate = new Translate(0, -460);
        info.style.scale = new Scale(Vector2.one);
        Set("BuildingName", so.GetBuildingName());
        Set("BuildingDescription", so.GetDescription());
        var grid = root.Q("PopUpGrid");
        grid.Clear();
        foreach (var stat in so.GetStats())
        {
            var tile = new VisualElement(); tile.AddToClassList("info-tile");
            var icon = new Label(stat["icon"]); icon.AddToClassList("tile-icon");
            var label = new Label(stat["label"]); label.AddToClassList("tile-label");
            var value = new Label(stat["value"]); value.AddToClassList("tile-value");
            tile.Add(icon); tile.Add(label); tile.Add(value); grid.Add(tile);
        }
    }
    private void ShowBuilding()
    {
        var so = selected.GetBuildingSO();
        root.Q("BuildingPopUp").style.display = DisplayStyle.Flex;
        Set("PopUpBuildingName", so != null ? so.GetBuildingName() : selectedObject.name);
        Set("RessourceNeeded1Text", selected.GetPrice().ToString());
        Set("RessourceNeeded2Text", selected.GetElectricityNeeded().ToString());
        Set("ProductionText", selected.GetRessource());
        Set("LevelText", "1");
        root.Q<Button>("SellButton").SetEnabled(selected is ISellable);
    }
    private void SellSelected()
    {
        if (selected is ISellable sellable) sellable.Sell();
        selected = null; selectedObject = null;
        root.Q("BuildingPopUp").style.display = DisplayStyle.None;
    }
}
