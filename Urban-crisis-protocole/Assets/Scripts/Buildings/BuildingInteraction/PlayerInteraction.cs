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
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            IBuildingUiData building = DetecteBuilding();
            if (building == null) return;
           
            
            panel.SetActive(true);
            panel.GetComponent<InfoPanelManager>().ReformatePanel(building);
        }
        
    }

    IBuildingUiData DetecteBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(hit.transform.name);
            if (hit.transform.TryGetComponent<IBuildingUiData>(out var data))
            {
                Debug.Log(hit.transform.name);
                return data;
            }
        }
        return null;
    }
}
