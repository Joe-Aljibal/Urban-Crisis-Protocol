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

    // UI Sections
    enum Sections
    {
        Houses,
        Businesses,
        Electricity,
        Water,
        Military,
        None
    };
    Sections currentSection;
    bool isPopUp = false;
    Coroutine hidingDescCoroutine = null;
    ViewModelCam viewModelCamera;
    [SerializeField] GameObject viewModelPivot;


    // Paths
    string mainPath = "Assets/Scripts/Buildings/ScriptableObjects";
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
        viewModelCamera = FindAnyObjectByType<ViewModelCam>();

        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(BuildingsPlacementUI);
        currentSection = Sections.None;

        buildingManager = gameManager.GetComponent<BuildingManager>();
        researchTreeUI = FindAnyObjectByType<ResearchTreeUI>();
    }

    void UpdateOwnedBuildings(Sections section)
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
            Debug.Log("here");
            var so = buildingSOs.FirstOrDefault(building => building.GetBuildingType() == type);
            if (so != null)
            {
                buildingsSelected.Add(so);
                Debug.Log("AAdded");
            }
        }
        Debug.Log($"[{section}] folderValid={AssetDatabase.IsValidFolder(folder)} " +
          $"guids={guids.Length} SOs={buildingSOs.Count} unlocked={unlocked.Count} " +
          $"selected={buildingsSelected.Count}");

        foreach (var b in buildingSOs) Debug.Log($"   found SO '{((ScriptableObject)b).name}' type={b.GetBuildingType()}");
        foreach (var t in unlocked) Debug.Log($"   unlocked: {t}");

        //foreach (var type in buildingsUnlocked)
        //{
        //    Debug.Log("Type at : " + type);
        //    foreach (var guid in guids)
        //    {
        //        string path = AssetDatabase.GUIDToAssetPath(guid);
        //        IBuildingSO asset = null;

        //        switch (section)
        //        {
        //            case Sections.Houses:
        //                asset = AssetDatabase.LoadAssetAtPath<HousesSO>(path);
        //                break;
        //            case Sections.Businesses:
        //                asset = AssetDatabase.LoadAssetAtPath<BusinessesSO>(path);
        //                break;
        //            case Sections.Electricity:
        //                asset = AssetDatabase.LoadAssetAtPath<ElectricitySO>(path);
        //                break;
        //            case Sections.Water:
        //                asset = AssetDatabase.LoadAssetAtPath<WaterSO>(path);
        //                break;
        //        }
        //        Debug.Log("Asset Type : " + asset.GetBuildingType());
        //        if (asset.GetBuildingType() == type)
        //        {
        //            Debug.Log(type);
        //            buildingsSelected.Add(asset);
        //            break;
        //        }
        //    }
        //    Debug.Log("index " + i++);
        //}
    }

    List<BuildingTypes> SelectList(Sections section)
    {
        PlayerInfo p = PlayerInfo.Instance;

        return section switch
        {
            Sections.Houses => p.housesUnlocked,
            Sections.Businesses => p.businessesUnlocked,
            Sections.Electricity => p.electricityUnlocked,
            Sections.Water => p.waterUnlocked,
            _ => null
        };
    }

    string SelectFolder(Sections section)
    {
        return section switch
        {
            Sections.Houses => housesPath,
            Sections.Businesses => businessesPath,
            Sections.Electricity => electricityPath,
            Sections.Water => waterPath,
            _ => null
        };
    }


    private void BuildingsPlacementUI(PanelRenderer renderer, VisualElement rootElement, int version)
    {
        root = rootElement;
        RefreshBuildingsSection(rootElement);
        SelectionSection(rootElement);
        SetPopupPosition(rootElement);
        rootElement.Q("background-zindex").SendToBack();
        StartCoroutine(HideDescriptionCoroutine(rootElement));
    }

    void RefreshBuildingsSection(VisualElement rootElement)
    {
        VisualElement scroller = rootElement.Q("BuildingsContainer");

        if (scroller == null)
            return;

        scroller.Clear();

        if (buildingsSelected == null || buildingsSelected.Count < 1)
            return;

        foreach (var so in buildingsSelected)
        {
            Button newButton = new Button();
            newButton.RegisterCallback<MouseEnterEvent>(e => { ShowDescription(rootElement, so); });
            newButton.RegisterCallback<MouseLeaveEvent>(e => { hidingDescCoroutine = StartCoroutine(HideDescriptionCoroutine(rootElement)); });

            if (so.GetIcon() != null)
            {
                newButton.style.backgroundImage = new StyleBackground(so.GetIcon());
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

            IBuildingSO selectedBuilding = so;

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

        housesCallback = _ => SelectorPressed(rootElement, Sections.Houses, houses);
        businessesCallback = _ => SelectorPressed(rootElement, Sections.Businesses, businesses);
        electricityCallback = _ => SelectorPressed(rootElement, Sections.Electricity, electricity);
        waterCallback = _ => SelectorPressed(rootElement, Sections.Water, water);
        researchCallback = _ => researchTreeUI.ToggleResearchPage();

        housesButton.RegisterCallback(housesCallback);
        businessesButton.RegisterCallback(businessesCallback);
        electricityButton.RegisterCallback(electricityCallback);
        waterButton.RegisterCallback(waterCallback);
        researchButton.RegisterCallback(researchCallback);
    }

    void SelectorPressed(VisualElement rootElement, Sections section, IEnumerable<IBuildingSO> buildings)
    {
        Debug.Log("Selector Pressed");
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

    void ShowDescription(VisualElement rootElement, IBuildingSO so)
    {
        Camera cam = GameObject.Find("3DModelViewCam").GetComponent<Camera>();
        // Handling 3d view model
        viewModelCamera.StartSpinning();
        foreach (Transform child in viewModelPivot.transform) { Destroy(child.gameObject); }
        GameObject model = Instantiate(so.GetCard().meshPrefab, Vector3.zero, Quaternion.identity, viewModelPivot.transform);
        model.layer = LayerMask.NameToLayer("3DModelView");
        cam.GetComponent<ViewModelCam>().Frame(model);
        foreach (Transform child in model.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("3DModelView");
        }

        if (hidingDescCoroutine != null)
        {
            StopCoroutine(hidingDescCoroutine);
            hidingDescCoroutine = null;
        }

        VisualElement descWindow = rootElement.Q("BuildingInfo");

        //descWindow.Q("ModelContainer").style.backgroundImage = new StyleBackground(so.GetIcon());
        descWindow.Q<Label>("BuildingName").text = so.GetBuildingName();
        descWindow.Q<Label>("BuildingDescription").text = so.GetDescription();
        var buildingStats = so.GetStats();

        rootElement.Q("PopUpGrid").Clear();
        foreach (var stat in buildingStats)
        {
            CreatePopUpGridElement(rootElement, stat);
        }

        descWindow.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        descWindow.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
        descWindow.style.translate = new Translate(0, -460);
        descWindow.style.scale = new Scale(new Vector2(1, 1));
    }

    VisualElement CreatePopUpGridElement(VisualElement rootElement, Dictionary<string, string> stat)
    {
        VisualElement parent = new VisualElement();
        parent.AddToClassList("info-tile");

        Label icon = new Label(stat["icon"]);
        icon.AddToClassList("tile-icon");

        Label label = new Label(stat["label"]);
        label.AddToClassList("tile-label");

        Label value = new Label(stat["value"]);
        value.AddToClassList("tile-value");

        parent.Add(icon);
        parent.Add(label);
        parent.Add(value);
        rootElement.Q("PopUpGrid").Add(parent);

        return parent;
    }

    System.Collections.IEnumerator HideDescriptionCoroutine(VisualElement rootElement)
    {
        yield return new WaitForSeconds(.25f);
        new WaitForSeconds(1f);
        VisualElement descWindow = rootElement.Q("BuildingInfo");

        descWindow.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        descWindow.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
        descWindow.style.translate = new Translate(0, 0);
        descWindow.style.scale = new Scale(new Vector2(0, 0));

        viewModelCamera.StopSpinning();
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
        currentSection = Sections.None;
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

    void UpdateInfoUI()
    {
        PlayerInfo playerInfo = PlayerInfo.Instance;
        Label moneyText = root.Q<Label>("MoneyValue");
        Label populationText = root.Q<Label>("PopulationValue");
        Label electricityText = root.Q<Label>("ElectricityValue");
        Label waterText = root.Q<Label>("WaterValue");
        Label foodText = root.Q<Label>("FoodValue");
        Label researchText = root.Q<Label>("ResearchPoints");

        moneyText.text = "$" + playerInfo.GetMoney.ToString();
        populationText.text = "<color=white>" + playerInfo.GetWorkingPopulation.ToString() + "</color>/" + playerInfo.GetPopulation.ToString();
        electricityText.text = "<color=white>" + playerInfo.GetElectricityUsed.ToString() + "</color>/" + playerInfo.GetElectricity.ToString();
        waterText.text = "<color=white>" + playerInfo.GetWaterUsed.ToString() + "</color>/" + playerInfo.GetWater.ToString();
        foodText.text = playerInfo.GetFood.ToString();
        researchText.text = "🧪 " + playerInfo.GetResearchPoints();
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            researchTreeUI.ToggleResearchPage();
        }

        UpdateInfoUI();
    }
}
