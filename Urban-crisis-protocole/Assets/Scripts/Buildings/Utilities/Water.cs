using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IBuilding
{
    [SerializeField] WaterSO waterSO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    Dictionary<House, int> waterDependants = new();
    bool isActive = true;
    int waterGiven = 0;

    void Start()
    {
        playerInfo.waterProviders.Add(this);
        playerInfo.StartProvidingWater();
    }

    public void DeleteButton()
    {
        playerInfo.AddMoney(waterSO.price * .8f);
        playerInfo.AddWater(-waterSO.water);
        Destroy(gameObject);
    }

    public void ProvideWater()
    {
        List<House> buildingsToRemove = new(0);

        foreach (House dependant in playerInfo.waterDependants)
        {
            if (waterDependants.ContainsKey(dependant))
                continue;

            if (!HasAvailableWater())
                break;

            int waterLeft = GetWaterOutput() - waterGiven;
            int waterNeeded = dependant.GetWaterNeeded();
            int amountToGive = Mathf.Min(waterLeft, waterNeeded);

            dependant.ReceiveWater(amountToGive);
            waterGiven += amountToGive;
            waterDependants.Add(dependant, amountToGive);

            if (dependant.HasEnoughWorkers())
                buildingsToRemove.Add(dependant);
        }

        foreach (House building in buildingsToRemove)
            playerInfo.electricityDependants.Remove(building);
    }

    public void SetActive()
    {
        isActive = true;
        playerInfo.AddWater(waterSO.water);
    }

    public void Deactivate()
    {
        isActive = false;
        playerInfo.AddWater(-waterSO.water);
    }

    public void Activate() { }
    public void Deactivate(int electricityLost, int workersLost) { throw new System.NotImplementedException(); }

    public void UseElectricity(int electricity) { }
    public void ReceiveWorkers(int workers) { }

    public bool CanPlace() => playerInfo.GetMoney >= waterSO.price;
    public int GetPrice() => waterSO.price;
    public IBuildingSO GetBuildingSO() => waterSO;
    public string GetBuildingType() => waterSO.type;
    public string GetRessource() => waterSO.ressource;

    public bool IsActive() => isActive;

    public int GetElectricityNeeded() => waterSO.electricityNeeded;
    public int GetElectricityReceived() => 0;
    public bool HasEnoughElectricity() => true;

    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;

    public int GetWaterOutput() => waterSO.water;
    public bool HasAvailableWater() => waterGiven < waterSO.water;
}