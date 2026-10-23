using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Water Silo", menuName = "ScriptableObjects/Water Silo")]
public class WaterSO : ScriptableObject, IBuildingSO
{
    public string type = "Utilities";
    public string ressource = "Water";
    public string buildingName;
    public string description;
    public int electricityNeeded = 0;
    public int water = 10;
    public int price = 25;
    public BuildingCard card;
    public Sprite icon;


    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney;
    public string GetBuildingName() => "💧 " + buildingName;

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>> {
        new Dictionary<string, string> {
            { "label" , "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Output" },
            { "icon", "💧" },
            { "value", water.ToString() }
        },
    };
    public string GetDescription() => description;
}
