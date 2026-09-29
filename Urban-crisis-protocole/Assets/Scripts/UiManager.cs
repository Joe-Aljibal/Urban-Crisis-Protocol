
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class UiManager : MonoBehaviour
{

    [SerializeField] private TMP_Text openBuildingMenuText;
     private string openingMenuText = "Press (E) to build";
     private string closingMenuText = "Press (E) to close Menu";
    [SerializeField] private GameObject buildingMenu;

    [SerializeField] TMP_Text elecShow;

    public event Action OnInteractWithMenu;

    void Start()
    {}


    void Update()
    {
        InteractWithMenu();

        elecShow.text = $"{PlayerInfo.Instance.getElectricityUsed} / {PlayerInfo.Instance.getElectricity}";
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


    void ChangeMenuText(bool value)
    {
        openBuildingMenuText.text = value ? openingMenuText : closingMenuText ;
    }
}