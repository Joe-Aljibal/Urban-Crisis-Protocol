using System.Collections.Generic;
using UnityEngine;

public class Electricity : MonoBehaviour, IBuilding, ISellable, IBuildingUiData
{
    [SerializeField] public ElectricitySO electricitySO;
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
    bool active;
    public int Output => electricitySO.GetElectricityOutput();
    public void ApplySupply(bool available) { active = available; }
    public void ProvideElectricity() => city.RecalculateBuildings();
    public void ElectrifyBuildings() => city.RecalculateBuildings();
    public void SetActive() => city.SetBuildingAvailable(this, true);
    public void Deactivate() => city.SetBuildingAvailable(this, false);
    public bool HasAvailableElectricity() => active && city.GetAvailableElectricity > 0;
    public int GetPrice() => electricitySO.GetPrice();
    public IBuildingSO GetBuildingSO() => electricitySO;
    public string GetRessource() => electricitySO.GetRessource();
    public string GetBuildingType() => "Electricity";
    public bool IsActive() => active;
    public int GetElectricityNeeded() => 0;
    public int GetElectricityReceived() => 0;
    public bool HasEnoughElectricity() => true;
    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;
    public List<UiDataModel> GetUiDataList() => new() { new UiDataModel(UiDataType.NAME, GetBuildingSO().GetBuildingName()), new UiDataModel(UiDataType.PRICE, GetPrice().ToString()) };

}
