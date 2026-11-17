using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IBuilding, ISellable, IBuildingUiData
{
    [SerializeField] WaterSO waterSO;
    CityData playerInfo = Cities.cities["PLAYER"];

    Dictionary<House, int> waterDependants = new();
    bool isActive = true;
    int waterGiven = 0;

    void Start()
    {
        playerInfo.waterProviders.Add(this);
        playerInfo.StartProvidingWater();

        playerInfo.AddWater(waterSO.GetWaterOutput());
    }

    public void DeleteButton()
    {
        playerInfo.AddMoney(waterSO.GetPrice() * .8f);
        playerInfo.AddWater(-waterSO.GetWaterOutput());
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
            playerInfo.UseWater(amountToGive);

            if (dependant.HasEnoughWorkers())
                buildingsToRemove.Add(dependant);
        }

        foreach (House building in buildingsToRemove)
            playerInfo.electricityDependants.Remove(building);
    }

    public void SetActive()
    {
        isActive = true;
        playerInfo.AddWater(waterSO.GetWaterOutput());
    }

    public void Deactivate()
    {
        isActive = false;
        playerInfo.AddWater(-waterSO.GetWaterOutput());
    }

    public void Activate() { }
    public void Deactivate(int electricityLost, int workersLost) { throw new System.NotImplementedException(); }

    public void ReceiveElectricity(int electricity) { }
    public void ReceiveWorkers(int workers) { }

    public bool CanPlace() => playerInfo.GetMoney >= waterSO.GetPrice();
    public int GetPrice() => waterSO.GetPrice();
    public IBuildingSO GetBuildingSO() => waterSO;
    public string GetBuildingType() => waterSO.GetBuildingType().ToString();
    public string GetRessource() => waterSO.GetRessource();

    public bool IsActive() => isActive;

    public int GetElectricityNeeded() => waterSO.GetElectricityNeeded();
    public int GetElectricityReceived() => 0;
    public bool HasEnoughElectricity() => true;

    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;

    public int GetWaterOutput() => waterSO.GetWaterOutput();
    public bool HasAvailableWater() => waterGiven < waterSO.GetWaterOutput();
    public void SetCityData(CityData data) => playerInfo = data ?? Cities.cities["PLAYER"];

    public void Sell() { playerInfo.RemoveBuilding(gameObject); DeleteButton(); }

    public List<UiDataModel> GetUiDataList() => new List<UiDataModel> {
        new UiDataModel(UiDataType.NAME, "Water"), new UiDataModel(UiDataType.PRICE, GetPrice().ToString()) };

}