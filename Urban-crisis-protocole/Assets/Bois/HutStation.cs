using UnityEngine;

public class HutStation : WorkStation, IBuilding
{
    private readonly HutStationsSO woodHutStationSO;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    protected override GameObject NpcPrefab { 
     get => base.NpcPrefab;
     set => base.NpcPrefab = value; }

    private Transform[] _resourceTransforms;

    bool isActive = true;

    public IBuildingSO GetBuildingSO() => woodHutStationSO;
    public bool CanPlace() =>
        playerInfo.getMoney >= woodHutStationSO.price &&
        playerInfo.getAvailableElectricity >= woodHutStationSO.electricityNeeded &&
        playerInfo.getAvailablePopulation >= woodHutStationSO.population;
    public int GetElectricityNeeded() => woodHutStationSO.electricityNeeded;
    public int GetPrice() => woodHutStationSO.price;
    public string GetRessource() => woodHutStationSO.resource;
    public string GetType() => woodHutStationSO.type;

    public bool IsActive() => isActive;

    public void SetActive()
    {
        isActive = true;
        playerInfo.addWood(woodHutStationSO.resourceAmount);
    }

    public void Deactivate()
    {
        isActive = false;
        playerInfo.addWood(-woodHutStationSO.resourceAmount);
    }

    public int GetWood => woodHutStationSO.resourceAmount;

    public void SetResourceTransforms(Transform[] resourceTransforms)
    {
        _resourceTransforms = resourceTransforms;
    }
    
    protected override Worker CreateSingleWorker(Vector3 position)
    {
        ResourceWorker resourceWorker = Instantiate(NpcPrefab, position, Quaternion.identity)
            .GetComponent<ResourceWorker>();

        resourceWorker.InitializeResourceWorker(_resourceTransforms, transform.position);

        return resourceWorker;
    }
}
