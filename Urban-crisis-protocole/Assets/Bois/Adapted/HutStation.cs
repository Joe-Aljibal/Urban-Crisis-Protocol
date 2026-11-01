using UnityEngine;

public class HutStation : MonoBehaviour, IBuilding
{
    private readonly HutStationsSO woodHutStationSO;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    bool isActive = true;

    public IBuildingSO GetBuildingSO() => woodHutStationSO;
    public bool CanPlace() =>
        playerInfo.GetMoney >= woodHutStationSO.price &&
        playerInfo.GetAvailableElectricity >= woodHutStationSO.electricityNeeded &&
        playerInfo.GetAvailablePopulation >= woodHutStationSO.population;
    public int GetElectricityNeeded() => woodHutStationSO.electricityNeeded;
    public int GetPrice() => woodHutStationSO.price;
    public string GetRessource() => woodHutStationSO.resource;
    public string GetBuildingType() => woodHutStationSO.type;
    public bool HasEnoughElectricity() => true;

    public bool IsActive() => isActive;

    public void SetActive()
    {
        isActive = true;
        playerInfo.AddWood(woodHutStationSO.resourceAmount);
    }

    public void Deactivate()
    {
        isActive = false;
        playerInfo.AddWood(-woodHutStationSO.resourceAmount);
    }

    public int GetElectricityReceived()
    {
        throw new System.NotImplementedException();
    }

    public void Deactivate(int electricityLost, int workersLost)
    {
        throw new System.NotImplementedException();
    }

    public void Activate()
    {
        throw new System.NotImplementedException();
    }

    public void UseElectricity(int electricity)
    {
        throw new System.NotImplementedException();
    }

    public int GetWood => woodHutStationSO.resourceAmount;
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;

    public bool HasAvailableWorkers() => true;
    public void DestroyScript() { enabled = false; }

  public bool HasEnoughWorkers()
  {
    throw new System.NotImplementedException();
  }
}
