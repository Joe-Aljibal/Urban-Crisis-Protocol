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

                Vector3[] occupied = card.placementRule.CanPlaceBuilding(pos, cityData.GetAllPositionStored(), w, h, groundGrid);
                if (occupied == null) continue;

                Vector3 exactPos = card.placementRule.ChangeFuturPosition(pos, w, h);
                Quaternion rot = Quaternion.Euler(0, rotated ? 90 : 0, 0);

                GameObject building = Instantiate(card.completPrefab, exactPos, rot);

                IBuilding buil = building.GetComponent<IBuilding>();
                buil.SetCityData(cityData);

                cityData.StoreNewBuilding(building, occupied.ToList());
                return true;
            }
        }
        return false;
    }

}
