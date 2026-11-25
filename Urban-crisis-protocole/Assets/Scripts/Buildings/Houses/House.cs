using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IBuilding, ISellable, IBuildingUiData
{
    [SerializeField] public HousesSO houseSO;
    CityData city = Cities.cities["PLAYER"];
    bool registered;
    void Start() { registered = true; city.RegisterBuilding(this); city.AddResearchPoints(houseSO.GetResearchOutput()); }
    void OnDestroy() { if (registered) { registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); } }
    public void SetCityData(CityData data) { city = data ?? Cities.cities["PLAYER"]; }
    public bool CanPlace() => city.GetMoney >= GetPrice();
    public void Sell() => DeleteButton();
    public void DeleteButton() { if (!registered) return; registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); city.AddMoney(GetPrice() * .8f); Destroy(gameObject); }
    public void Activate() => city.SetBuildingAvailable(this, true);
    public void Deactivate(int electricityLost, int workersLost) => city.SetBuildingAvailable(this, false);
    public void ReceiveElectricity(int value) => city.RecalculateBuildings();
    public void ReceiveWorkers(int value) => city.RecalculateBuildings();
    int power, water; bool active;
    public int PowerDemand => houseSO.GetElectricityNeeded();
    public int WaterDemand => houseSO.GetWaterNeeded();
    public void ApplySupply(int p, int w, bool intact, bool available) { power = p; water = w; active = intact && available && HasEnoughElectricity() && HasEnoughWater(); }
    public void ProvideWorkers() => city.RecalculateBuildings();
    public void ReceiveWater(int amount) => city.RecalculateBuildings();
    public void SetActive() => city.SetBuildingAvailable(this, true);
    public void Deactivate() => city.SetBuildingAvailable(this, false);
    public int GetPopulation() => houseSO.GetPopulation();
    public int GetPrice() => houseSO.GetPrice();
    public IBuildingSO GetBuildingSO() => houseSO;
    public string GetRessource() => houseSO.GetRessource();
    public string GetBuildingType() => "House";
    public bool IsActive() => active;
    public int GetElectricityNeeded() => Mathf.Max(0, PowerDemand - power);
    public int GetElectricityReceived() => power;
    public bool HasEnoughElectricity() => power >= PowerDemand;
    public int GetWaterNeeded() => Mathf.Max(0, WaterDemand - water);
    public bool HasEnoughWater() => water >= WaterDemand;
    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => active;
    public List<UiDataModel> GetUiDataList() => new() { new UiDataModel(UiDataType.NAME, GetBuildingSO().GetBuildingName()), new UiDataModel(UiDataType.PRICE, GetPrice().ToString()) };

}
