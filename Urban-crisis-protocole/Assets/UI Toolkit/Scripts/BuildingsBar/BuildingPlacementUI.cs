using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class BuildingPlacementUI : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance; 
    private PanelRenderer panelRenderer;
    List<IBuildingSO> buildingsSelected = new();
    [SerializeField] GameObject gameManager;

    BuildingClasses currentSection;
    bool isPopUp = false;

    // Paths
    string housesPath = "Assets/Scripts/Buildings/ScriptableObjects/Houses";
    string businessesPath = "Assets/Scripts/Buildings/ScriptableObjects/Businesses";
    string electricityPath = "Assets/Scripts/Buildings/ScriptableObjects/Utilities/Electricity";
    string waterPath = "Assets/Scripts/Buildings/ScriptableObjects/Utilities/Water";

    // ScriptableObject Arrays
    List<IBuildingSO> houses = new();
    List<IBuildingSO> businesses = new();
    List<IBuildingSO> electricity = new();
    List<IBuildingSO> water = new();

    VisualElement root;
    BuildingManager buildingManager;

    EventCallback<ClickEvent> housesCallback;
    EventCallback<ClickEvent> businessesCallback;
    EventCallback<ClickEvent> electricityCallback;
    EventCallback<ClickEvent> waterCallback;

    // Research UI
    ResearchTreeUI researchTreeUI;
    EventCallback<ClickEvent> researchCallback;

    private void Awake()
    {

        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(BuildingsPlacementUI);
        currentSection = BuildingClasses.None;

        buildingManager = gameManager.GetComponent<BuildingManager>();
        researchTreeUI = FindAnyObjectByType<ResearchTreeUI>();
    }

    void UpdateOwnedBuildings(BuildingClasses section)
    {
        buildingsSelected.Clear();

        List<BuildingTypes> unlocked = SelectList(section);
        string folder = SelectFolder(section);

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { folder });
        List<IBuildingSO> buildingSOs = new();

        foreach (string guid in guids)
        {
            IBuildingSO so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)) as IBuildingSO;
            if (so != null) buildingSOs.Add(so);
        }

        foreach(var type in unlocked)
        {
            var so = buildingSOs.FirstOrDefault(building => building.GetBuildingType() == type);
            if (so != null)
            {
                buildingsSelected.Add(so);
            }
        }
    }

    List<BuildingTypes> SelectList(BuildingClasses section)
    {
        PlayerInfo p = PlayerInfo.Instance;

        return section switch
        {
            BuildingClasses.House => p.housesUnlocked,
            BuildingClasses.Business => p.businessesUnlocked,
            BuildingClasses.Electricity => p.electricityUnlocked,
            BuildingClasses.Water => p.waterUnlocked,
            _ => null
        };
    }

    string SelectFolder(BuildingClasses section)
    {
        return section switch
        {
            BuildingClasses.House => housesPath,
            BuildingClasses.Business => businessesPath,
            BuildingClasses.Electricity => electricityPath,
            BuildingClasses.Water => waterPath,
            _ => null
        };
    }


    private void BuildingsPlacementUI(PanelRenderer renderer, VisualElement rootElement, int version)
    {
        root = rootElement;
        RefreshBuildingsSection(rootElement);
        SelectionSection(rootElement);
        SetPopupPosition(rootElement);
        rootElement.Q("BuildingInfo").SendToBack();
    }

    void RefreshBuildingsSection(VisualElement rootElement)
    {
        BuildingDescriptionUI uiScript = FindAnyObjectByType<BuildingDescriptionUI>();

        VisualElement scroller = rootElement.Q("BuildingsContainer");

        if (scroller == null)
            return;

        scroller.Clear();

        if (buildingsSelected == null || buildingsSelected.Count < 1)
            return;

        foreach (var building in buildingsSelected)
        {

            Button newButton = new Button();
            newButton.RegisterCallback<MouseEnterEvent>(e => { uiScript.ShowDescription(rootElement, building); });
            newButton.RegisterCallback<MouseLeaveEvent>(e => { uiScript.HideDescription(rootElement); });

            if (building.GetIcon() != null)
            {
                newButton.style.backgroundImage = new StyleBackground(building.GetIcon());
                newButton.style.backgroundColor = new Color(0, 0, 0, 0);
            }

            newButton.style.width = 80;
            newButton.style.height = 80;
            newButton.style.borderBottomLeftRadius = 10;
            newButton.style.borderBottomRightRadius = 10;
            newButton.style.borderTopLeftRadius = 10;
            newButton.style.borderTopRightRadius = 10;
            newButton.style.paddingBottom = 5;
            newButton.style.paddingTop = 5;
            newButton.style.paddingLeft = 5;
            newButton.style.paddingRight = 5;

            IBuildingSO selectedBuilding = building;

            newButton.clicked += () =>
                buildingManager.ChangeCurrentBuildingCard(selectedBuilding.GetCard());

            scroller.Add(newButton);
        }
    }

    void SelectionSection(VisualElement rootElement)
    {
        Button housesButton = rootElement.Q<Button>("HousesSelector");
        Button businessesButton = rootElement.Q<Button>("BusinessesSelector");
        Button electricityButton = rootElement.Q<Button>("ElectricitySelector");
        Button waterButton = rootElement.Q<Button>("WaterSelector");
        Button researchButton = rootElement.Q<Button>("ResearchTreeButton");

        if (housesCallback != null)
            housesButton.UnregisterCallback(housesCallback);

        if (businessesCallback != null)
            businessesButton.UnregisterCallback(businessesCallback);

        if (electricityCallback != null)
            electricityButton.UnregisterCallback(electricityCallback);

        if (waterCallback != null)
            waterButton.UnregisterCallback(waterCallback);

        if (researchCallback != null)
            researchButton.UnregisterCallback(researchCallback);

        housesCallback = _ => SelectorPressed(rootElement, BuildingClasses.House, houses);
        businessesCallback = _ => SelectorPressed(rootElement, BuildingClasses.Business, businesses);
        electricityCallback = _ => SelectorPressed(rootElement, BuildingClasses.Electricity, electricity);
        waterCallback = _ => SelectorPressed(rootElement, BuildingClasses.Water, water);
        researchCallback = _ => researchTreeUI.ToggleResearchPage();

        housesButton.RegisterCallback(housesCallback);
        businessesButton.RegisterCallback(businessesCallback);
        electricityButton.RegisterCallback(electricityCallback);
        waterButton.RegisterCallback(waterCallback);
        researchButton.RegisterCallback(researchCallback);
    }

    void SelectorPressed(VisualElement rootElement, BuildingClasses section, IEnumerable<IBuildingSO> buildings)
    {
        if (isPopUp && currentSection == section)
        {
            HideBuildings(rootElement);
            return;
        }

        currentSection = section;
        buildingsSelected = buildings.ToList();
        
        UpdateOwnedBuildings(section);

        RefreshBuildingsSection(rootElement);

        if (!isPopUp)
        {
            ShowBuildings(rootElement);
        }
    }

    void ShowBuildings(VisualElement rootElement)
    {
        isPopUp = true;

        VisualElement scroller = rootElement.Q<ScrollView>("BuildingsContainer");

        scroller.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        scroller.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        scroller.style.translate = new Translate(0, -120);
    }

    void HideBuildings(VisualElement rootElement)
    {
        isPopUp = false;
        currentSection = BuildingClasses.None;
        buildingsSelected = new();
        RefreshBuildingsSection(rootElement);

        VisualElement scroller = rootElement.Q<ScrollView>("BuildingsContainer");

        scroller.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        scroller.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        scroller.style.translate = new Translate(0, 0);
    }

    void SetPopupPosition(VisualElement rootElement)
    {
        VisualElement scroller = rootElement.Q<ScrollView>("BuildingsContainer");

        if (scroller == null)
            return;

        scroller.style.transitionDuration = new List<TimeValue>
        {
            new TimeValue(0, TimeUnit.Millisecond)
        };

        scroller.style.translate = isPopUp
            ? new Translate(0, 0)
            : new Translate(0, 120);
    }

    private void OnDestroy()
    {
        panelRenderer.UnregisterUIReloadCallback(BuildingsPlacementUI);
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            researchTreeUI.ToggleResearchPage();
        }
    }
}
