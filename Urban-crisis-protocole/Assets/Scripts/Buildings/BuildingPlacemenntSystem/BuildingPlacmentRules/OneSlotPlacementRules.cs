using UnityEngine;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;



[CreateAssetMenu(menuName = "Building/PlacementRules/OneSlotPlacementRules")]
public class OneSlotPlacementRules : BuildingPlacementRules
{
    public override Vector3[] CanPlaceBuilding(Vector3 futurPosition , List<Vector3> filledSlot)
    {

        if (FindPositionInList(futurPosition, filledSlot))  return null;


        Vector3[] occupiedSlots = {futurPosition};
        return occupiedSlots;
    }
    public override Vector3 ChangeFuturPosition(Vector3 futurPosition)
    {
        return futurPosition;
    }
}
