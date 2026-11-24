
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class PlayerController : MonoBehaviour
{

    private FindPositionWiithMouseArray finder = new FindPositionWiithMouseArray();
    [SerializeField] private GameObject movableGrid;
    private GameObject currentSelectedSlot;
    private BuildingCard currentBuidlingCard;
    private GameObject currentSelectedBuildingClone;
    [SerializeField] private Material[] materials = new Material[2];
    private CityData cityData;
    
    private UiManager uiManager;

    private BuildingManager manager;

   void Start()
    {
        manager = GetComponent<BuildingManager>();
        uiManager = GetComponent<UiManager>();
        if (uiManager != null) uiManager.OnInteractWithMenu += ResetDefaultBuilding;
        cityData = Cities.cities["PLAYER"];
    }
    void Update()
    {
        if (currentBuidlingCard != null && Mouse.current != null && Keyboard.current != null)
        {
            MoveGridAround(finder.FindPosition());
        }
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
    public bool rotate = false;

    void InteractWithPreviewBuilding(Vector3 position)
    {

        int currentWidth = currentBuidlingCard.width;
        int currentHeight = currentBuidlingCard.height;


        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentSelectedBuildingClone.transform.Rotate(0, 90, 0);
            rotate = !rotate;
            
        }

        currentWidth = rotate ? currentBuidlingCard.height : currentBuidlingCard.width;
        currentHeight = rotate ?  currentBuidlingCard.width : currentBuidlingCard.height;
  
        
        // !! faux parce que CanPlaceBuildig 
        Vector3[] positionOccupiedByBuilding = currentBuidlingCard.GetOccupiedCells(position, cityData.GetAllPositionStored(), finder.CurrentGrid, rotate);

        Vector3 exactPosition = currentBuidlingCard.GetPlacementPosition(position, rotate);
        currentSelectedSlot.transform.position = exactPosition;
        currentSelectedBuildingClone.transform.position = exactPosition;
        




        if (positionOccupiedByBuilding == null)
        {
            ChangeBuildingColor(materials[1]);
        }
        else
        {
            ChangeBuildingColor(materials[0]);
            // is used to check if the mouse is on a button
            if (Mouse.current.leftButton.wasPressedThisFrame && !FindUiButtonUnderMouse()
                && manager.TryPlaceForCity(currentBuidlingCard, position, currentSelectedBuildingClone.transform.rotation, cityData, finder.CurrentGrid))
                ResetDefaultBuilding();

        }
    }

    bool FindUiButtonUnderMouse()
    {
        if (CityFireEvent.IsPointerOverPanel || ResourceUIManager.IsPointerOverDashboard || CityConstruction.IsPointerOverPanel) return true;
        if (EventSystem.current == null || Mouse.current == null) return false;
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (RaycastResult ui in results)
        {
            if (ui.gameObject.GetComponent<Button>()) return true;
        }

        return false;
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

    public void ChangeCurrentBuildingCard(BuildingCard selectBuildingCard ) 
    {
        ResetDefaultBuilding();

        cityData ??= Cities.cities["PLAYER"];
        manager ??= GetComponent<BuildingManager>();
        rotate = false;
        currentBuidlingCard = selectBuildingCard;
        InitializeBuildingPlacement();
    }

    void InitializeBuildingPlacement()
    {
        Vector3? position = finder.FindPosition();

        currentSelectedBuildingClone = Instantiate(currentBuidlingCard.meshPrefab);
        currentSelectedSlot = Instantiate(currentBuidlingCard.prefabCurrentCaseSelection);
        MoveGridAround(position);
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
