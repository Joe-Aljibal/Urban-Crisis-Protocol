using UnityEngine;


[CreateAssetMenu(menuName = "Building/Card")]
public class BuildingCard : ScriptableObject
{

   public GameObject prefabCurrentCaseSelection;
   public GameObject completPrefab;
   public GameObject meshPrefab;
   public BuildingPlacementRules placementRule;
}
