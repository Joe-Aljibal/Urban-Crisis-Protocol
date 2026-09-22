using UnityEngine;

[CreateAssetMenu(fileName = "Business", menuName = "ScriptableObjects/Business")]
public class BusinessesSO : ScriptableObject, IBuildingSO
{
    public string type = "Business";
    public string ressource = "Money";
    public int electricityNeeded;
    public int population;
    public int price;
    public int profit;

    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney && electricityNeeded <= PlayerInfo.Instance.getAvailableElectricity && population <= PlayerInfo.Instance.getAvailablePopulation;
}
