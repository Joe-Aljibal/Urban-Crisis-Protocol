using UnityEngine;

public interface IBuilding
{
    public void DestroyScript();
    public bool CanPlace();
    public int GetPrice();
    public IBuildingSO GetBuildingSO();
    public string GetBuildingType();
    public string GetRessource();

    public bool IsActive();
    public void Activate();
    public void Deactivate(int electricityLost, int workersLost);

    public void UseElectricity(int electricity);
    public int GetElectricityNeeded();
    public int GetElectricityReceived();
    public bool HasEnoughElectricity();

    public void ReceiveWorkers(int workers);
    public int GetWorkersNeeded();
    public bool HasEnoughWorkers();
}