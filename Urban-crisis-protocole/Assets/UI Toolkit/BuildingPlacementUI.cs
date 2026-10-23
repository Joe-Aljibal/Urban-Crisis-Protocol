using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingPlacementUI : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    IBuildingSO[] buildingsSelected;
    [SerializeField] GameObject gameManager;

    // UI Sections
    enum Sections
    {
        Houses,
        Businesses,
        Industries,
        Food,
        Electricity,
        Water,
        None
    };
    Sections currentSection;
    bool isPopUp = false;
    Coroutine hidingDescCoroutine = null;
    ViewModelCam viewModelCamera;
    [SerializeField] GameObject viewModelPivot;


    // Paths
    string housesPath = "Assets/Scripts/Buildings/ScriptableObjects/Houses";
    string businessesPath = "Assets/Scripts/Buildings/ScriptableObjects/Businesses";
    string electricityPath = "Assets/Scripts/Buildings/ScriptableObjects/Electricity";
    string waterPath = "Assets/Scripts/Buildings/ScriptableObjects/Water";

    // ScriptableObject Arrays
    List<HousesSO> houses = new();
    List<BusinessesSO> businesses = new();
    List<ElectricitySO> electricity = new();
    List<WaterSO> water = new();

    VisualElement root;
    BuildingManager buildingManager;

    EventCallback<ClickEvent> housesCallback;
    EventCallback<ClickEvent> businessesCallback;
    EventCallback<ClickEvent> electricityCallback;
    EventCallback<ClickEvent> waterCallback;

    private void Awake()
    {
        viewModelCamera = FindAnyObjectByType<ViewModelCam>();

        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(BuildingsPlacementUI);
        currentSection = Sections.None;

        buildingManager = gameManager.GetComponent<BuildingManager>();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { housesPath, businessesPath, electricityPath, waterPath });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            switch (path)
            {
                case var _ when path.Contains("Houses"):
                    HousesSO asset1 = AssetDatabase.LoadAssetAtPath<HousesSO>(path);
                    houses.Add(asset1);
                    break;
                case var _ when path.Contains("Businesses"):
                    BusinessesSO asset2 = AssetDatabase.LoadAssetAtPath<BusinessesSO>(path);
                    businesses.Add(asset2);
                    break;
                case var _ when path.Contains("Electricity"):
                    ElectricitySO asset3 = AssetDatabase.LoadAssetAtPath<ElectricitySO>(path);
                    electricity.Add(asset3);
                    break;
                case var _ when path.Contains("Water"):
                    WaterSO asset4 = AssetDatabase.LoadAssetAtPath<WaterSO>(path);
                    water.Add(asset4);
                    break;
            }
        }
    }

    private void BuildingsPlacementUI(PanelRenderer renderer, VisualElement rootElement, int version)
    {
        root = rootElement;
        RefreshBuildingsSection(rootElement);
        SelectionSection(rootElement);
        SetPopupPosition(rootElement);
        StartCoroutine(HideDescriptionCoroutine(rootElement));
    }

    void RefreshBuildingsSection(VisualElement rootElement)
    {
        VisualElement scroller = rootElement.Q("BuildingsContainer");

        if (scroller == null)
            return;

        scroller.Clear();

        if (buildingsSelected == null || buildingsSelected.Length < 1)
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

        if (housesCallback != null)
            housesButton.UnregisterCallback(housesCallback);

        if (businessesCallback != null)
            businessesButton.UnregisterCallback(businessesCallback);

        if (electricityCallback != null)
            electricityButton.UnregisterCallback(electricityCallback);

        if (waterCallback != null)
            waterButton.UnregisterCallback(waterCallback);

        housesCallback = _ => SelectorPressed(rootElement, Sections.Houses, houses);
        businessesCallback = _ => SelectorPressed(rootElement, Sections.Businesses, businesses);
        electricityCallback = _ => SelectorPressed(rootElement, Sections.Electricity, electricity);
        waterCallback = _ => SelectorPressed(rootElement, Sections.Water, water);

        housesButton.RegisterCallback(housesCallback);
        businessesButton.RegisterCallback(businessesCallback);
        electricityButton.RegisterCallback(electricityCallback);
        waterButton.RegisterCallback(waterCallback);
    }

    void SelectorPressed(VisualElement rootElement, Sections section, IEnumerable<IBuildingSO> buildings)
    {
        if (isPopUp && currentSection == section)
        {
            ClosePopUp(rootElement);
            return;
        }

        currentSection = section;
        buildingsSelected = buildings.ToArray();
        RefreshBuildingsSection(rootElement);

        if (!isPopUp)
        {
            ShowPopUp(rootElement);
        }
    }

    void ShowDescription(VisualElement rootElement, IBuildingSO so)
    {
        // Handling 3d view model
        viewModelCamera.StartSpinning();
        foreach ( Transform child in viewModelPivot.transform ) { Destroy(child.gameObject); }
        GameObject model = Instantiate(so.GetCard().meshPrefab, Vector3.zero, Quaternion.identity, viewModelPivot.transform);
        model.layer = LayerMask.NameToLayer("3DModelView");
        foreach(Transform child in model.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("3DModelView");
        }
        //model.transform.localScale = new Vector3(1,1,1);

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
        foreach ( var stat in buildingStats )
        {
            CreatePopUpGridElement(rootElement, stat);
        }

        descWindow.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        descWindow.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
        descWindow.style.translate = new Translate(0, 0);
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
        descWindow.style.translate = new Translate(0, 320);
        descWindow.style.scale = new Scale(new Vector2(0.1f, 0.1f));

        viewModelCamera.StopSpinning();
    }

    void ShowPopUp(VisualElement rootElement)
    {
        isPopUp = true;

        VisualElement scroller = rootElement.Q<ScrollView>("BuildingsContainer");

        scroller.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        scroller.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        scroller.style.translate = new Translate(0, 0);
    }

    void ClosePopUp(VisualElement rootElement)
    {
        isPopUp = false;
        currentSection = Sections.None;
        buildingsSelected = new IBuildingSO[] { };
        RefreshBuildingsSection(rootElement);

        VisualElement scroller = rootElement.Q<ScrollView>("BuildingsContainer");

        scroller.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        scroller.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        scroller.style.translate = new Translate(0, 120);
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

        moneyText.text = "$" + playerInfo.getMoney.ToString();
        populationText.text = playerInfo.getPopulation.ToString();
        electricityText.text = playerInfo.getElectricity.ToString();
        waterText.text = playerInfo.getWater.ToString();
        foodText.text = playerInfo.getFood.ToString();
    }

    private void Update()
    {
        UpdateInfoUI();
    }
}