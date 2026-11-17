using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    
    private GameObject currentBuildingSelected;
    
    void Start()
    {}

    // !! make sure mthe real p^refab are having a boxcollider or else there is no detection
    void Update()
    {
        InteractWithBuilding();
    }


    void InteractWithBuilding()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame && panel != null)
        {
            IBuildingUiData building = DetecteBuilding();
            if (building == null) return;
           
            
            panel.SetActive(true);
            panel.GetComponent<InfoPanelManager>().ReformatePanel(building, currentBuildingSelected);
        }
        
    }

    IBuildingUiData DetecteBuilding()
    {
        if (Camera.main == null || Mouse.current == null) return null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(hit.transform.name);
            if (hit.transform.TryGetComponent<IBuildingUiData>(out var data))
            {
                Debug.Log(hit.transform.name);
                currentBuildingSelected = hit.transform.gameObject;
                return data;
            }
        }
        return null;
    }
}
