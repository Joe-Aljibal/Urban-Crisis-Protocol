using UnityEngine;

public class HutStation : MonoBehaviour, IBuilding
{
    private readonly HutStationsSO woodHutStationSO;

    PlayerInfo playerInfo = PlayerInfo.Instance;

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
}
