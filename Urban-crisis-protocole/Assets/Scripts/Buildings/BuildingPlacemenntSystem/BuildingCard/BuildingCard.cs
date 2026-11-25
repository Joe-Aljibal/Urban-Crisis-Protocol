using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "Building/Card")]
public class BuildingCard : ScriptableObject
{
   public GameObject prefabCurrentCaseSelection;
   public GameObject completPrefab;
   public GameObject meshPrefab;
   public BuildingPlacementRules placementRule;
   public int width = 1; 
   public int height = 1;
    public Vector3[] GetOccupiedCells(Vector3 position, List<Vector3> occupied, Grid grid, bool rotated)
    {
        if (placementRule == null) return null;
        int w = Mathf.Max(1, rotated ? height : width);
        int h = Mathf.Max(1, rotated ? width : height);
        return placementRule.CanPlaceBuilding(position, occupied, w, h, grid);
    }
    public Vector3 GetPlacementPosition(Vector3 position, bool rotated)
    {
        return placementRule.ChangeFuturPosition(position, Mathf.Max(1, rotated ? height : width), Mathf.Max(1, rotated ? width : height));
    }

}
