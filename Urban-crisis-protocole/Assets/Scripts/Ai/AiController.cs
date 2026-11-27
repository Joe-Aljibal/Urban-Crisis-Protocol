using UnityEngine;
using System.Linq;

public class AiController : MonoBehaviour
{
    [SerializeField] private GameObject ground;     
    private BuildingManager manager;
    private CityData cityData;
    private FindPositionWithScan finder;
    private Grid groundGrid;

    void Start()
    {
        manager = GetComponent<BuildingManager>();
        cityData = Cities.cities["AI"];
        groundGrid = ground.GetComponent<Grid>();
        finder = new FindPositionWithScan(ground);
    }

    void Update()
    {}


    // make a list of card / call it / add back all component needed to the gameManager   make reward/action and train 
    public bool TryPlaceBuilding(BuildingCard card)
    {
        finder.Reset();

        for (int i = 0; i < finder.CellCount; i++)
        {
            Vector3 pos = finder.FindPosition().Value;

            for (int r = 0; r < 2; r++)
            {
                bool rotated = r == 1;
                int w = rotated ? card.height : card.width;
                int h = rotated ? card.width : card.height;

                Vector3[] occupied = card.GetOccupiedCells(pos, cityData.GetAllPositionStored(), groundGrid, rotated);
                if (occupied == null) continue;

                Quaternion rot = Quaternion.Euler(0, rotated ? 90 : 0, 0);
                if (manager.TryPlaceForCity(card, pos, rot, cityData, groundGrid)) return true;

            }
        }
        return false;
    }

}
