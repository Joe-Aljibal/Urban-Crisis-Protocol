using UnityEngine;

public interface IBuilding
{
    public string getType();
    public string getRessource();
    public int getElectricityNeeded();
    public int getPrice();
    public bool canPlace();
    public IBuildingSO getBuildingSO();
}
