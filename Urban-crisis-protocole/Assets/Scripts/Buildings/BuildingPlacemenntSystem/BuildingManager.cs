
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
    Dictionary<GameObject, List<Vector3>> buildingsPositions = new Dictionary<GameObject, List<Vector3>>();
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
        gridComponent = gridPlateform.GetComponent<Grid>();
        uiManager = GetComponent<UiManager>();
        if (uiManager != null) uiManager.OnInteractWithMenu += ResetDefaultBuilding;

    }

    void Update()
    {
        if (currentBuidlingCard != null && Mouse.current != null && Keyboard.current != null)
        {
            MoveGridAround(FindMousePositionOnGrid());
        }

    }

    Vector3? DetecteGround()
    {
        if (Camera.main == null || Mouse.current == null) return null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Collider buildArea = gridPlateform.GetComponent<Collider>();
        return buildArea != null && buildArea.Raycast(ray, out RaycastHit hit, 1000f)
            ? hit.point : (Vector3?)null;
    }

    Vector3? FindMousePositionOnGrid()
    {

        Vector3? position = DetecteGround();
        if (position == null) return position;
        Vector3 correctPos = position.Value;


        Vector3 caseCenter = ConvertWorldToGrid(correctPos);
        caseCenter.y = gridPlateform.transform.position.y + 0.01f;

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

        Vector3­[] positionOccupiedByBuilding = currentBuidlingCard.placementRule.CanPlaceBuilding(position, GetAllPositionStored());

        Vector3 exactPosition = currentBuidlingCard.placementRule.ChangeFuturPosition(position);
        currentSelectedSlot.transform.position = exactPosition;

        currentSelectedBuildingClone.transform.position = exactPosition;


        if (!CityConstruction.IsMenuOpen && Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentSelectedBuildingClone.transform.Rotate(0, 90, 0);
        }



        if (positionOccupiedByBuilding == null)
        {
            ChangeBuildingColor(materials[1]);
        }
        else
        {
            ChangeBuildingColor(materials[0]);
            // trouver si bouton
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !FindUiButtonUnderMouse())
                TryPlace(currentBuidlingCard, position, currentSelectedBuildingClone.transform.rotation);
        }
    }

    public void RotatePreview()
    {
        if (currentSelectedBuildingClone != null) currentSelectedBuildingClone.transform.Rotate(0, 90, 0);
    }

    // Places one existing card immediately. Returns false without charging when blocked.
    public bool TryPlace(BuildingCard card, Vector3 gridPosition, Quaternion rotation)
    {
        if (card == null || card.completPrefab == null || card.placementRule == null) return false;
        if (gridComponent == null) gridComponent = gridPlateform.GetComponent<Grid>();
        Vector3[] occupied = card.placementRule.CanPlaceBuilding(gridPosition, GetAllPositionStored());
        if (occupied == null) { Status = "This space is occupied."; return false; }
        IBuilding building = card.completPrefab.GetComponent<IBuilding>();
        int cost = building != null ? building.GetPrice() : 0;
        if (PlayerInfo.Instance.getMoney < cost) { Status = "Not enough money ($" + cost + ")."; return false; }
        Vector3 position = card.placementRule.ChangeFuturPosition(gridPosition);
        GameObject placed = Instantiate(card.completPrefab, position, rotation);
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
        PlayerInfo.Instance.addMoney(-cost);
        buildingsPositions[placed] = new List<Vector3>(occupied);
        Status = card.name + " built ($" + cost + ").";
        ResetDefaultBuilding();
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
        Vector3 center = gridPlateform.transform.position;
        Quaternion rotation = currentSelectedBuildingClone != null ? currentSelectedBuildingClone.transform.rotation : Quaternion.identity;
        int[] offsets = { 0, 4, -4, 8, -8, 12, -12 };
        foreach (int z in offsets)
        {
            foreach (int x in offsets)
            {
                Vector3 position = new Vector3(center.x + x, center.y + 0.01f, center.z + z);
                if (TryPlace(card, position, rotation)) return true;
            }
        }
        Status = "No open space in the build area.";
        return false;
    }

    bool FindUiButtonUnderMouse()
    {
        if (CityFireEvent.IsPointerOverPanel || ResourceUIManager.IsPointerOverDashboard || CityConstruction.IsPointerOverPanel) return true;
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

    List<Vector3> GetAllPositionStored()
    {
        List<Vector3> positions = new List<Vector3>();
        List<GameObject> removed = new List<GameObject>();
        foreach (var variable in buildingsPositions)
        {
            if (variable.Key == null) { removed.Add(variable.Key); continue; }
            List<Vector3> positionsStored = variable.Value;

            foreach (Vector3 positionStored in positionsStored)
            {
                positions.Add(positionStored);
            }
        }
        foreach (GameObject building in removed) buildingsPositions.Remove(building);
        return positions;
    }

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


        currentSelectedBuildingClone = Instantiate(currentBuidlingCard.meshPrefab);
        currentSelectedSlot = Instantiate(currentBuidlingCard.prefabCurrentCaseSelection);

        MoveGridAround(position);
    }


    public void ChangeCurrentBuildingCard(BuildingCard selectBuildingCard)
    {
        ResetDefaultBuilding();

        Status = "Click the area or press Place building.";
        currentBuidlingCard = selectBuildingCard;
        CreateBuilding();
    }

    public void ResetDefaultBuilding()
    {
        if (currentSelectedBuildingClone != null)
        {
            Destroy(currentSelectedBuildingClone);
            Destroy(currentSelectedSlot);
            movableGrid.SetActive(false);
            currentBuidlingCard = null;
        }
    }

    private void OnDestroy()
    {
        if (uiManager != null) uiManager.OnInteractWithMenu -= ResetDefaultBuilding;
        if (currentSelectedBuildingClone != null) Destroy(currentSelectedBuildingClone);
        if (currentSelectedSlot != null) Destroy(currentSelectedSlot);
    }












}
