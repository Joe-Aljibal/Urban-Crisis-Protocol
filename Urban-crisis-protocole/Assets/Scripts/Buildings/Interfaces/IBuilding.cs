using UnityEngine;

public interface IBuilding
{
    public string GetBuildingType();
    public string GetRessource();
    public void UseElectricity(int electricity);
    public int GetElectricityReceived();
    public int GetElectricityNeeded();
    public bool HasEnoughElectricity();
    public int GetPrice();
    public bool CanPlace();
    public IBuildingSO GetBuildingSO();

    public bool IsActive();
    public void Deactivate(int electricityLost);
    public void Activate();
}
