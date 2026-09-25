
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class UiManager : MonoBehaviour
{

    [SerializeField] private TMP_Text openBuildingMenuText;
     private string openingMenuText = "Press (E) to build";
     private string closingMenuText = "Press (E) to close Menu";
    [SerializeField] private GameObject buildingMenu;

    [SerializeField] private GameObject workStationMenu;
    private WorkStationUI workStationUI;

    public event Action OnInteractWithMenu;

    void Start()
    {
        workStationUI = GetComponent<WorkStationUI>();
    }


    void Update()
    {
        InteractWithMenu();
        InteractWithWorkStation();
    }

    void InteractWithMenu()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            bool isOpen = buildingMenu.activeSelf;
            buildingMenu.SetActive(!isOpen);
            
            ChangeMenuText(isOpen);

            OnInteractWithMenu?.Invoke();

        }
    }

    // Handle Interactions with WorkStation buildings (OnClick)
    void InteractWithWorkStation()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            workStationMenu.SetActive(false);
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.transform.TryGetComponent(out WorkStation workStation) && Mouse.current.leftButton.wasPressedThisFrame)
            {
                workStationMenu.SetActive(true);
                workStationUI.SetCurrentBuilding(workStation);
            }
        }
    }


    void ChangeMenuText(bool value)
    {
        openBuildingMenuText.text = value ? openingMenuText : closingMenuText ;
    }



}