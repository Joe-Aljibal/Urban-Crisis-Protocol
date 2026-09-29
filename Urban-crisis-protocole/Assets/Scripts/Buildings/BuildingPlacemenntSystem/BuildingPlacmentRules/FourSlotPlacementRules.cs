using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "Building/PlacementRules/FourSlotPlacementRules")]
public class FourSlotPlacementRules : BuildingPlacementRules
{


    public override Vector3[] CanPlaceBuilding(Vector3 futurPosition, List<Vector3> filledSlot)
    {
        // would be better to do it depending on the way the player is looking

        // if i am over the the edge  iam coocked  
         Vector3[] occupiedSlot = {futurPosition, futurPosition + new Vector3(0.5f, 0, 0.5f),futurPosition + new Vector3(0f, 0, 0.5f), 
                                 futurPosition + new Vector3(0.5f, 0, 0f) };

        foreach (Vector3 v in occupiedSlot)
        { 
            if (FindPositionInList(v, filledSlot)) return null;
        }

        return occupiedSlot;
    }
    
    public override Vector3 ChangeFuturPosition(Vector3 futurPosition)
    {
        futurPosition += new Vector3(0.25f, 0, 0.25f);
        return futurPosition;
    }


}
