using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IBuilding, ISellable, IBuildingUiData
{
    [SerializeField] WaterSO waterSO;
    CityData city = Cities.cities["PLAYER"];
    bool registered;
    void Start() { registered = true; city.RegisterBuilding(this);  }
    void OnDestroy() { if (registered) { registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); } }
    public void SetCityData(CityData data) { city = data ?? Cities.cities["PLAYER"]; }
    public bool CanPlace() => city.GetMoney >= GetPrice();
    public void Sell() => DeleteButton();
    public void DeleteButton() { if (!registered) return; registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); city.AddMoney(GetPrice() * .8f); Destroy(gameObject); }
    public void Activate() => city.SetBuildingAvailable(this, true);
    public void Deactivate(int electricityLost, int workersLost) => city.SetBuildingAvailable(this, false);
    public void ReceiveElectricity(int value) => city.RecalculateBuildings();
    public void ReceiveWorkers(int value) => city.RecalculateBuildings();
    int power; bool active;
    public int PowerDemand => waterSO.GetElectricityNeeded();
    public void ApplySupply(int supplied, bool available) { power = supplied; active = available && HasEnoughElectricity(); }
    public void ProvideWater() => city.RecalculateBuildings();
    public void SetActive() => city.SetBuildingAvailable(this, true);
    public void Deactivate() => city.SetBuildingAvailable(this, false);
    public int GetWaterOutput() => waterSO.GetWaterOutput();
    public bool HasAvailableWater() => active && city.GetAvailableWater > 0;
    public int GetPrice() => waterSO.GetPrice();
    public IBuildingSO GetBuildingSO() => waterSO;
    public string GetRessource() => waterSO.GetRessource();
    public string GetBuildingType() => waterSO.GetBuildingType().ToString();
    public bool IsActive() => active;
    public int GetElectricityNeeded() => Mathf.Max(0, PowerDemand - power);
    public int GetElectricityReceived() => power;
    public bool HasEnoughElectricity() => power >= PowerDemand;
    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;
    public List<UiDataModel> GetUiDataList() => new() { new UiDataModel(UiDataType.NAME, GetBuildingSO().GetBuildingName()), new UiDataModel(UiDataType.PRICE, GetPrice().ToString()) };

}
