using UnityEngine;

public interface IBuilding
{
    public string GetType();
    public string GetRessource();
    public int GetElectricityNeeded();
    public int GetPrice();
    public bool CanPlace();
    public IBuildingSO GetBuildingSO();

    public bool IsActive();
    public void SetActive();
    public void Deactivate();
}
