using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Business", menuName = "ScriptableObjects/Business")]
public class BusinessesSO : ScriptableObject, IBuildingSO
{
    public string type = "Business";
    public string ressource = "Money";
    public string buildingName;
    public string description;
    public Sprite icon;
    public int electricityNeeded;
    public int workersNeeded;
    public int price;
    public int profit;
    public BuildingCard card;

    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney;
    public string GetBuildingName() => "💵 " + buildingName;

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>>
    {
        new Dictionary<string, string> {
            { "label" , "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Profit" },
            { "icon", "💵" },
            { "value", profit.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Workers" },
            { "icon", "👥" },
            { "value", workersNeeded.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Consumes" },
            { "icon", "⚡" },
            { "value", electricityNeeded.ToString() }
        },
    };

    public string GetDescription() => description;
}
