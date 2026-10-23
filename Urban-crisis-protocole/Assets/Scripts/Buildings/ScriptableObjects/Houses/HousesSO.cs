using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "House", menuName = ("ScriptableObjects/House"))]
public class HousesSO : ScriptableObject, IBuildingSO
{
    public string type = "Population";
    public string ressource = "Population";
    public string buildingName;
    public string description;
    public int electricityNeeded;
    public int waterNeeded;
    public int population;
    public int price;
    public BuildingCard card;
    public Sprite icon;

    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;

    PlayerInfo playerInfo = PlayerInfo.Instance;
    public string GetBuildingName() => "🏡 " + buildingName; 
     public bool CanPlace() =>
        playerInfo.getMoney >= price &&
        playerInfo.getAvailableElectricity >= electricityNeeded;  // !! water 

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>>
    {
        new Dictionary<string, string> {
            { "label" , "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Population" },
            { "icon", "🏡" },
            { "value", population.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Consumes" },
            { "icon", "⚡" },
            { "value", electricityNeeded.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Consumes" },
            { "icon", "💧" },
            { "value", waterNeeded.ToString() }
        },
    };
    public string GetDescription() => description;
}
