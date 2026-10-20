using System;
using UnityEngine;

public class AIOptions : MonoBehaviour
{
    [SerializeField] BuildingManager buildingManager;
    [SerializeField] private BuildingCard houseCard;
    [SerializeField] private BuildingCard gasStationCard;

    [SerializeField] private GridHelper grid;

    void Start()
    {
        AIEvents.OnAISpawnHouse += AISpawnHouse;
        AIEvents.OnAISpawnGasStation += AISpawnGasStation;
        AIEvents.OnAICollectWood += AICollectWood;
        AIEvents.OnAICollectStone += AICollectStone;
    }

    public void AISpawnHouse()
    {
        Debug.LogWarning("Spawn house was called");
        GameObject newBuilding = Instantiate(
            houseCard.completPrefab,
            grid.GetRandomCell(),
            houseCard.completPrefab.transform.rotation);
    }

    public void AISpawnGasStation()
    {
        GameObject newBuilding = Instantiate(
           gasStationCard.completPrefab,
           grid.GetRandomCell(),
           gasStationCard.completPrefab.transform.rotation);
    }
    public void AICollectWood()
    {

    }
    public void AICollectStone()
    {

    }
}
