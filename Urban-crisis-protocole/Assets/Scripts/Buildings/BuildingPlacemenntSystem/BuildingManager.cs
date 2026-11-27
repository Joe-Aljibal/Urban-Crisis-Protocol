
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class BuildingManager : MonoBehaviour
{

    [SerializeField] private GameObject gridPlateform;
    [SerializeField] private GameObject movableGrid;
    private Grid gridComponent;
    private GameObject currentSelectedSlot;
    private CityData cityData => Cities.cities["PLAYER"];
    private BuildingCard currentBuidlingCard;
    private GameObject currentSelectedBuildingClone;
    [SerializeField] private Material[] materials = new Material[2];

    private UiManager uiManager;
    public string Status { get; private set; } = "Choose a building, then place it.";
    public bool HasSelection => currentBuidlingCard != null;

    public void Configure(GameObject platform, GameObject previewGrid, Material valid, Material invalid)
    {
        gridPlateform = platform;
        movableGrid = previewGrid;
        materials = new[] { valid, invalid };
    }


    void Start()
    {
        gridComponent = gridPlateform != null ? gridPlateform.GetComponent<Grid>() : null;
        uiManager = GetComponent<UiManager>();
        if (uiManager != null) uiManager.OnInteractWithMenu += ResetDefaultBuilding;

    }

    void Update()
    {
        if (currentBuidlingCard == null || Mouse.current == null) return;
        if (Mouse.current.rightButton.isPressed)
        {
            ResetDefaultBuilding();
            Status = "Placement annulé.";
            return;
        }
        MoveGridAround(FindMousePositionOnGrid());
    }

    Vector3? DetecteGround()
    {
        if (Camera.main == null || Mouse.current == null) return null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        var surface = gridPlateform != null ? gridPlateform.GetComponent<CityBuildSurface>() : null;
        if (surface != null) return surface.Raycast(ray, out RaycastHit hit) ? hit.point : (Vector3?)null;
        Collider buildArea = gridPlateform != null ? gridPlateform.GetComponent<Collider>() : null;
        return buildArea != null && buildArea.Raycast(ray, out RaycastHit fallback, 2000f) ? fallback.point : (Vector3?)null;
    }

    Vector3? FindMousePositionOnGrid()
    {

        Vector3? position = DetecteGround();
        if (position == null) return position;
        Vector3 correctPos = position.Value;


        Vector3 caseCenter = ConvertWorldToGrid(correctPos);
        caseCenter.y = position.Value.y + 0.01f;

        return caseCenter;
    }

    Vector3 ConvertWorldToGrid(Vector3 position)
    {

        Vector3Int cell = gridComponent.WorldToCell(position);
        Vector3 caseCenter = gridComponent.GetCellCenterWorld(cell);

        return caseCenter;
    }

    void MoveGridAround(Vector3? position)
    {
        bool isGostState = UpdateGhostState(position);
        if (isGostState) return;


        movableGrid.transform.position = position.Value;
        InteractWithPreviewBuilding(position.Value);
    }

    bool UpdateGhostState(Vector3? position)
    {
        if (position == null)
        {
            movableGrid.SetActive(false);
            currentSelectedBuildingClone.SetActive(false);
            currentSelectedSlot.SetActive(false);
            return true;
        }
        else
        {
            movableGrid.SetActive(true);
            currentSelectedBuildingClone.SetActive(true);
            currentSelectedSlot.SetActive(true);
            return false;
        }


    }


    public bool canBuild = true;
    void InteractWithPreviewBuilding(Vector3 position)
    {
        if (!CityConstruction.IsMenuOpen && Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
            RotatePreview();
        Quaternion rotation = currentSelectedBuildingClone.transform.rotation;
        bool rotated = Mathf.RoundToInt(rotation.eulerAngles.y / 90f) % 2 != 0;
        bool valid = ValidatePlacement(currentBuidlingCard, position, rotation, cityData, gridComponent, out _, out Vector3 exactPosition, out string reason);
        currentSelectedSlot.transform.position = exactPosition;
        currentSelectedBuildingClone.transform.position = exactPosition;
        ChangeBuildingColor(materials[valid ? 0 : 1]);
        Status = valid ? "Emplacement valide — clique pour construire." : reason;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !FindUiButtonUnderMouse())
            TryPlace(currentBuidlingCard, position, rotation);
    }

    public void RotatePreview()
    {
        if (currentSelectedBuildingClone != null) currentSelectedBuildingClone.transform.Rotate(0, 90, 0);
    }

    // Places one existing card immediately. Returns false without charging when blocked.
    public bool TryPlace(BuildingCard card, Vector3 gridPosition, Quaternion rotation)
    {
        if (gridComponent == null && gridPlateform != null) gridComponent = gridPlateform.GetComponent<Grid>();
        bool placed = TryPlaceForCity(card, gridPosition, rotation, cityData, gridComponent);
        if (placed) ResetDefaultBuilding();
        return placed;
    }

    public GameObject CreateBuilding(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject building = Instantiate(prefab, position, rotation);
        building.GetComponent<IBuilding>()?.SetCityData(cityData);
        return building;
    }

    private bool ValidatePlacement(BuildingCard card, Vector3 gridPosition, Quaternion rotation, CityData owner, Grid grid,
        out Vector3[] occupied, out Vector3 position, out string reason)
    {
        occupied = null; position = gridPosition; reason = "Construction invalide.";
        if (card == null || card.completPrefab == null || card.placementRule == null || owner == null || grid == null) return false;
        var building = card.completPrefab.GetComponent<IBuilding>();
        if (building == null) return false;
        int cost = building.GetPrice();
        if (cost < 0 || owner.GetMoney < cost)
        { reason = "Fonds insuffisants : besoin de $" + cost + " (disponible : $" + owner.FormattedMoney + ")."; return false; }
        bool rotated = Mathf.RoundToInt(rotation.eulerAngles.y / 90f) % 2 != 0;
        var allOccupied = new List<Vector3>();
        foreach (CityData city in Cities.cities.Values) allOccupied.AddRange(city.GetAllPositionStored());
        if (!Cities.cities.ContainsValue(owner)) allOccupied.AddRange(owner.GetAllPositionStored());
        occupied = card.GetOccupiedCells(gridPosition, allOccupied, grid, rotated);
        position = card.GetPlacementPosition(gridPosition, rotated, grid);
        if (occupied == null) { reason = "Emplacement occupé ou hors carte."; return false; }
        var surface = grid.GetComponent<CityBuildSurface>();
        if (surface != null)
        {
            if (!surface.Validate(occupied, grid.cellSize.x, out float height, out reason)) return false;
            position.y = height + .01f;
        }
        return true;
    }

    public bool TryPlaceForCity(BuildingCard card, Vector3 gridPosition, Quaternion rotation, CityData owner, Grid grid)
    {
        if (!ValidatePlacement(card, gridPosition, rotation, owner, grid, out Vector3[] occupied, out Vector3 position, out string reason))
        { Status = reason; return false; }
        int cost = card.completPrefab.GetComponent<IBuilding>().GetPrice();
        if (!owner.TrySpend(cost)) { Status = "Fonds insuffisants."; return false; }
        GameObject placed = Instantiate(card.completPrefab, position, rotation);
        placed.GetComponent<IBuilding>()?.SetCityData(owner);
        CityStructure condition = placed.GetComponent<CityStructure>();
        if (condition == null) condition = placed.AddComponent<CityStructure>();
        condition.Configure("Construction", Color.white);
        FindAnyObjectByType<CityPowerOutage>()?.IncludeBuilding(placed);
        if (placed.GetComponentInChildren<Collider>() == null)
        {
            Renderer renderer = placed.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                BoxCollider collider = placed.AddComponent<BoxCollider>();
                collider.center = placed.transform.InverseTransformPoint(renderer.bounds.center);
                Vector3 scale = placed.transform.lossyScale;
                collider.size = new Vector3(renderer.bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)), renderer.bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)), renderer.bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(scale.z)));
            }
        }
        owner.StoreNewBuilding(placed, new List<Vector3>(occupied));
        Status = card.name + " built ($" + cost + ").";
        return true;
    }

    // An explicit button can place the selection without relying on a precise mouse ray.
    public bool PlaceSelectedInBuildArea()
    {
        if (currentBuidlingCard == null || gridPlateform == null)
        {
            Status = "Choose a building first.";
            return false;
        }
        BuildingCard card = currentBuidlingCard;
        IBuilding building = card.completPrefab != null ? card.completPrefab.GetComponent<IBuilding>() : null;
        if (building != null && PlayerInfo.Instance.getMoney < building.GetPrice())
        {
            Status = "Not enough money ($" + building.GetPrice() + ").";
            return false;
        }
        var surface = gridPlateform.GetComponent<CityBuildSurface>();
        Bounds bounds = surface != null ? surface.Bounds : new Bounds(gridPlateform.transform.position, Vector3.one * 20f);
        Quaternion rotation = currentSelectedBuildingClone != null ? currentSelectedBuildingClone.transform.rotation : Quaternion.identity;
        Vector3 center = ConvertWorldToGrid(bounds.center);
        int maxRadius = Mathf.CeilToInt(Mathf.Max(bounds.extents.x, bounds.extents.z) / 4f);
        for (int ring = 0; ring <= maxRadius; ring++)
        {
            for (int z = -ring; z <= ring; z++)
                for (int x = -ring; x <= ring; x++)
                {
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) != ring) continue;
                    Vector3 position = center + new Vector3(x * 4f, 0f, z * 4f);
                    if (position.x < bounds.min.x || position.x > bounds.max.x || position.z < bounds.min.z || position.z > bounds.max.z) continue;
                    if (TryPlace(card, position, rotation)) return true;
                }
        }
        Status = "No open space in the build area.";
        return false;
    }

    bool FindUiButtonUnderMouse()
    {
        if (CityFireEvent.IsPointerOverPanel || IntegratedGameControls.IsPointerOverCasino || ResourceUIManager.IsPointerOverDashboard || CityConstruction.IsPointerOverPanel) return true;
        if (EventSystem.current == null) return false;
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (RaycastResult ui in results)
        {
            if (ui.gameObject.GetComponent<Button>()) return true;
        }

        return false;
    }

    List<Vector3> GetAllPositionStored() => cityData.GetAllPositionStored();

    void ChangeBuildingColor(Material material)
    {
        Transform[] children = currentSelectedBuildingClone.GetComponentsInChildren<Transform>();
        foreach (Transform t in children)
        {
            MeshRenderer renderer = t.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.material = material;
        }
    }


    void CreateBuilding()
    {
        Vector3? position = FindMousePositionOnGrid();


        // Some cards use gameplay prefabs as their preview. Keep preview components
        // from producing resources or blocking the placement ray.
        var inactiveParent = new GameObject("Inactive preview container");
        inactiveParent.SetActive(false);
        currentSelectedBuildingClone = Instantiate(currentBuidlingCard.meshPrefab, inactiveParent.transform);
        currentSelectedSlot = Instantiate(currentBuidlingCard.prefabCurrentCaseSelection, inactiveParent.transform);
        foreach (var behaviour in inactiveParent.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
        foreach (var collider in inactiveParent.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        currentSelectedBuildingClone.transform.SetParent(null);
        currentSelectedSlot.transform.SetParent(null);
        Destroy(inactiveParent);
        MoveGridAround(position);
    }


    public void ChangeCurrentBuildingCard(BuildingCard selectBuildingCard)
    {
        ResetDefaultBuilding();

        var building = selectBuildingCard != null && selectBuildingCard.completPrefab != null
            ? selectBuildingCard.completPrefab.GetComponent<IBuilding>() : null;
        Status = building != null
            ? "Prix : $" + building.GetPrice() + " | Fonds : $" + cityData.FormattedMoney + " — clique sur le terrain."
            : "Choisis un emplacement sur le terrain.";
        currentBuidlingCard = selectBuildingCard;
        if (currentBuidlingCard != null) CreateBuilding();
    }

    public void ResetDefaultBuilding()
    {
        if (currentSelectedBuildingClone != null) { currentSelectedBuildingClone.SetActive(false); Destroy(currentSelectedBuildingClone); }
        if (currentSelectedSlot != null) { currentSelectedSlot.SetActive(false); Destroy(currentSelectedSlot); }
        if (movableGrid != null) movableGrid.SetActive(false);
        currentSelectedBuildingClone = null;
        currentSelectedSlot = null;
        currentBuidlingCard = null;
    }

    private void OnDestroy()
    {
        if (uiManager != null) uiManager.OnInteractWithMenu -= ResetDefaultBuilding;
        if (currentSelectedBuildingClone != null) Destroy(currentSelectedBuildingClone);
        if (currentSelectedSlot != null) Destroy(currentSelectedSlot);
    }












}
