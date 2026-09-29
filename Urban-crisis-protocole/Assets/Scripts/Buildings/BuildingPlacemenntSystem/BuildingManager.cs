
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

     
    void Start()
    {
        gridComponent = gridPlateform.GetComponent<Grid>();
        uiManager = GetComponent<UiManager>();
        uiManager.OnInteractWithMenu += ResetDefaultBuilding;

    }

    void Update()
    {
        if (currentBuidlingCard != null)
        {
            MoveGridAround(FindMousePositionOnGrid());
        }

    }

    Vector3? DetecteGround()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 mousePosition = hit.point;
            if (hit.transform.gameObject != gridPlateform) return null;

            return mousePosition;
        }
        return null;
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


        if (Keyboard.current.sKey.wasPressedThisFrame)
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
            if (Mouse.current.leftButton.IsPressed() && !FindUiButtonUnderMouse())
            {
                movableGrid.SetActive(false);
                Destroy(currentSelectedSlot);
                GameObject newBuilding = Instantiate(currentBuidlingCard.completPrefab, currentSelectedBuildingClone.transform.position, currentSelectedBuildingClone.transform.rotation);
                

                // !! special section (hut)
                /*
                if (newBuilding.TryGetComponent(out LocalHutManager r))
                 {
                      r.resource manager = 
                 }
                */

                
                Destroy(currentSelectedBuildingClone);
                currentSelectedBuildingClone = null;
                currentBuidlingCard = null;

                buildingsPositions[newBuilding] = new List<Vector3> { newBuilding.transform.position };

                foreach (Vector3 v in positionOccupiedByBuilding)
                {
                    buildingsPositions[newBuilding].Add(v);
                }

            }
        }
    }

    bool FindUiButtonUnderMouse()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

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
        foreach (var variable in buildingsPositions)
        {
            List<Vector3> positionsStored = buildingsPositions[variable.Key];

            foreach (Vector3 positionStored in positionsStored)
            {
                positions.Add(positionStored);
            }
        }
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












}
