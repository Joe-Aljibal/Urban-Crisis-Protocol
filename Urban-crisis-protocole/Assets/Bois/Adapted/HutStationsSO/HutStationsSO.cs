using UnityEngine;

[CreateAssetMenu(fileName = "Water Silo", menuName = "ScriptableObjects/Hut Station")]
public class HutStationsSO : ScriptableObject, IBuildingSO
{
    public string type = "Utilities";
    public string resource;
    public int electricityNeeded = 0;
    public int population;
    public int resourceAmount = 1;
    public int price = 25;

    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney;
}
