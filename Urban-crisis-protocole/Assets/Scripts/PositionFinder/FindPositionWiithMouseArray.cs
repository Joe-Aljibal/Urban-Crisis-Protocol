using UnityEngine;
using UnityEngine.InputSystem;

public class FindPositionWiithMouseArray : IFindPosition
{

    private GameObject gridPlateform;
    private Grid gridComponent;

    public Grid CurrentGrid { get; private set; } 

    public Vector3? FindPosition()
    {
        Vector3? position = DetecteGround();
        if (position == null) return position;
        Vector3 correctPos = position.Value;


        Vector3 caseCenter = ConvertWorldToGrid(correctPos);
        caseCenter.y = gridPlateform.transform.position.y + 0.01f;

        CurrentGrid = gridComponent;   
        
        // reset de gridPlateform
        gridPlateform = null;
        gridComponent = null;

        return caseCenter;
    }

    Vector3? DetecteGround()
    {
        if (Camera.main == null || Mouse.current == null) return null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 mousePosition = hit.point;
            if (hit.transform.gameObject.TryGetComponent<Grid>(out var grid))
            {
                gridPlateform = grid.transform.gameObject;
                gridComponent = grid;
                return mousePosition;
            }
        }
        return null;
    }

     Vector3 ConvertWorldToGrid(Vector3 position)
    {

        Vector3Int cell = gridComponent.WorldToCell(position);
        Vector3 caseCenter = gridComponent.GetCellCenterWorld(cell);

        return caseCenter;
    }

    
   
}
