using System.Collections.Generic;
using UnityEngine;

public class HutStation : MonoBehaviour, IBuilding, ISellable
{
    [SerializeField] private HutStationsSO woodHutStationSO;
    [SerializeField] private float collectionInterval = 5f;
    [SerializeField] private float collectionRadius = 35f;
    private CityData city = Cities.cities["PLAYER"];
    private int power, workers;
    private bool active, registered;
    private float elapsed;
    public int PowerDemand => woodHutStationSO != null ? woodHutStationSO.electricityNeeded : 0;
    public int WorkerDemand => woodHutStationSO != null ? woodHutStationSO.population : 0;
    private ResourceType ProducedType => woodHutStationSO != null && System.Enum.TryParse(woodHutStationSO.resource, true, out ResourceType type) ? type : ResourceType.Wood;

    private void Start() { if (woodHutStationSO != null) { registered = true; city.RegisterBuilding(this); } }
    private void OnDestroy() { if (registered) { registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject); } }
    private void Update()
    {
        if (!active || woodHutStationSO == null) return;
        elapsed += Time.deltaTime;
        float interval = Mathf.Max(.1f, collectionInterval);
        while (elapsed >= interval)
        {
            elapsed -= interval;
            StaticResource nearest = null;
            float best = collectionRadius * collectionRadius;
            foreach (Transform candidate in ResourceManager.GetResourceTransforms(ProducedType))
            {
                if (candidate == null) continue;
                var resource = candidate.GetComponent<StaticResource>();
                float distance = (candidate.position - transform.position).sqrMagnitude;
                if (resource != null && resource.IsActiveResource && resource.IsAvailable && distance <= best)
                { nearest = resource; best = distance; }
            }
            if (nearest == null) break;
            nearest.Disable();
            city.collectResource(ProducedType, Mathf.Max(0, woodHutStationSO.resourceAmount));
        }
    }
    public void ApplySupply(int suppliedPower, int suppliedWorkers, bool available)
    { power = suppliedPower; workers = suppliedWorkers; active = available && HasEnoughElectricity() && HasEnoughWorkers(); }
    public void SetCityData(CityData data) => city = data ?? Cities.cities["PLAYER"];
    public IBuildingSO GetBuildingSO() => woodHutStationSO;
    public bool CanPlace() => woodHutStationSO != null && city.GetMoney >= GetPrice();
    public int GetPrice() => woodHutStationSO != null ? woodHutStationSO.price : 0;
    public string GetRessource() => woodHutStationSO != null ? woodHutStationSO.resource : "";
    public string GetBuildingType() => woodHutStationSO != null ? woodHutStationSO.type : "";
    public int GetElectricityNeeded() => Mathf.Max(0, PowerDemand - power);
    public int GetElectricityReceived() => power;
    public bool HasEnoughElectricity() => power >= PowerDemand;
    public int GetWorkersNeeded() => Mathf.Max(0, WorkerDemand - workers);
    public bool HasEnoughWorkers() => workers >= WorkerDemand;
    public bool HasAvailableWorkers() => HasEnoughWorkers();
    public bool IsActive() => active;
    public int GetWood => woodHutStationSO != null ? woodHutStationSO.resourceAmount : 0;
    public void SetActive() => Activate();
    public void Activate() => city.SetBuildingAvailable(this, true);
    public void Deactivate() => city.SetBuildingAvailable(this, false);
    public void Deactivate(int electricityLost, int workersLost) => Deactivate();
    public void ReceiveElectricity(int electricity) => city.RecalculateBuildings();
    public void ReceiveWorkers(int value) => city.RecalculateBuildings();
    public void Sell()
    {
        if (!registered) return;
        registered = false; city.UnregisterBuilding(this); city.RemoveBuilding(gameObject);
        city.AddMoney(GetPrice() * .8f); Destroy(gameObject);
    }
}
