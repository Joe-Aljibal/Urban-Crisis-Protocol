using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Business", menuName = "ScriptableObjects/Business")]
public class BusinessesSO : ScriptableObject, IBuildingSO
{
    public string type = "Business";
    public string ressource = "Money";
    public Sprite icon;
    public int electricityNeeded;
    public int population;
    public int price;
    public int profit;
    public BuildingCard card;

    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney && electricityNeeded <= PlayerInfo.Instance.getAvailableElectricity && population <= PlayerInfo.Instance.getAvailablePopulation;
}
