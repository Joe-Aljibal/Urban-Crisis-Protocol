using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Building/PlacementRules")]
public abstract class BuildingPlacementRules : ScriptableObject
{
   public abstract Vector3[] CanPlaceBuilding(Vector3 futurPosition, List<Vector3> filledSlot);

   public abstract Vector3 ChangeFuturPosition(Vector3 futurPosition);

    public bool FindPositionInList(Vector3 position, List<Vector3> filledSlot)
    {
        foreach (Vector3 p in filledSlot)
        {
            if(p == position)
            {
                return true;
            }
        }
        return false;
    }
}
