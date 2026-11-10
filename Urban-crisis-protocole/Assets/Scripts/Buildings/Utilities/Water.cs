using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IBuilding , IBuildingUiData, ISellable
{
    [SerializeField] WaterSO waterSO;
    CityData cityData;
    bool isActive = true;
    
    void Start()
    {
        cityData.addWater(waterSO.water);
    }

    public void DeleteButton()
    {
        cityData.addMoney(waterSO.price * .8f);
        cityData.addWater(-waterSO.water);

        Destroy(gameObject);
    }

    public IBuildingSO GetBuildingSO() => waterSO;
    public bool CanPlace() => waterSO.price <= cityData.getMoney;
    public int GetElectricityNeeded() => waterSO.electricityNeeded;
    public int GetPrice() => waterSO.price;
    public string GetRessource() => waterSO.ressource;
    public bool HasEnoughElectricity() => true;
    public string GetBuildingType() => waterSO.type;
    public int GetWater => waterSO.water;
    public bool IsActive() => isActive;

    public void SetActive()
    {
        isActive = true;
        cityData.addWater(waterSO.water);
    }

    public void SetCityData(CityData cityData)
    {
        this.cityData = cityData;
    }

    public void Deactivate() {
        isActive = false;
        cityData.addWater(-waterSO.water);
    }

    public int GetElectricityReceived() => 0;

    public void Deactivate(int electricityLost, int workersLost)
    {
        throw new System.NotImplementedException();
    }

    public List<UiDataText> GetUiTextList()
    {
        
        return new List<UiDataText>
        { 
           
        };
    }

    public List<UiDataInteraction> GetUiInteractionList()
    {
        return new List<UiDataInteraction>
        {
            new UiDataInteraction(UiInteractionType.SELL, new Sell())
        };
    }
    public void Activate() { }
    public void UseElectricity(int electricity) { }
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;

    public bool HasEnoughWorkers() => true;

    public void Sell()
    {}
}
