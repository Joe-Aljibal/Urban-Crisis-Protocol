using UnityEngine;

public interface IBuilding
{
    public string GetType();
    public string GetRessource();
    public int GetElectricityReceived();
    public int GetElectricityNeeded();
    public int GetPrice();
    public bool CanPlace();
    public IBuildingSO GetBuildingSO();

    public bool IsActive();
    public void Deactivate(int electricityLost);
    public void Activate();
    public void UseElectricity(int electricity);
}
