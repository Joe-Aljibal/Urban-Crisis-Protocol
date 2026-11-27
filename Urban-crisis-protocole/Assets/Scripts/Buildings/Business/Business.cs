using System.Collections.Generic;
using UnityEngine;

public class Business : MonoBehaviour, IBuilding, ISellable, IBuildingUiData
{
    [SerializeField] BusinessesSO businessSO;
    CityData city = Cities.cities["PLAYER"];
    bool registered;
    void Start() { registered = true; city.RegisterBuilding(this); city.AddResearchPoints(businessSO.GetResearchOutput()); }
    void OnDestroy() { if (registered) { registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); } }
    public void SetCityData(CityData data) { city = data ?? Cities.cities["PLAYER"]; }
    public bool CanPlace() => city.GetMoney >= GetPrice();
    public void Sell() => DeleteButton();
    public void DeleteButton() { if (!registered) return; registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); city.AddMoney(GetPrice() * .8f); Destroy(gameObject); }
    public void Activate() => city.SetBuildingAvailable(this, true);
    public void Deactivate(int electricityLost, int workersLost) => city.SetBuildingAvailable(this, false);
    public void ReceiveElectricity(int value) => city.RecalculateBuildings();
    public void ReceiveWorkers(int value) => city.RecalculateBuildings();
    int power; public int workersReceived; bool active; float cooldown;
    public int PowerDemand => businessSO.GetElectricityNeeded();
    public int WorkerDemand => businessSO.GetWorkersNeeded();
    public void ApplySupply(int p, int workers, bool available) { power = p; workersReceived = workers; active = available && HasEnoughElectricity() && HasEnoughWorkers(); }
    void Update()
    {
        if (!active) { cooldown = 0; return; }
        cooldown += Time.deltaTime;
        int seconds = Mathf.FloorToInt(cooldown);
        if (seconds > 0) { city.AddMoney(businessSO.GetProfit() * seconds); cooldown -= seconds; }
    }
    public int GetProfit() => businessSO.GetProfit();
    public int GetWorkerPopulation() => workersReceived;
    public int GetPopulation() => WorkerDemand;
    public int GetPrice() => businessSO.GetPrice();
    public IBuildingSO GetBuildingSO() => businessSO;
    public string GetRessource() => businessSO.GetRessource();
    public bool IsActive() => active;
    public int GetElectricityNeeded() => Mathf.Max(0, PowerDemand - power);
    public int GetElectricityReceived() => power;
    public bool HasEnoughElectricity() => power >= PowerDemand;
    public int GetWorkersNeeded() => Mathf.Max(0, WorkerDemand - workersReceived);
    public bool HasEnoughWorkers() => workersReceived >= WorkerDemand;
    public List<UiDataModel> GetUiDataList() => new() { new UiDataModel(UiDataType.NAME, GetBuildingSO().GetBuildingName()), new UiDataModel(UiDataType.PRICE, GetPrice().ToString()) };

}
