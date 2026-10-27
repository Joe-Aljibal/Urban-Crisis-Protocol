using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ElectricitySO", menuName = "ScriptableObjects/ElectricitySO")]
public class ElectricitySO : ScriptableObject, IBuildingSO
{
    public string type = "Utility";
    public string ressource = "Electricity";
    public string buildingName;
    public string description;
    public int electricityOutput;
    public int price;
    public BuildingCard card;
    public Sprite icon;

    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney;
    public string GetBuildingName() => "⚡ " + buildingName;

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>>
    {
        new Dictionary<string, string> {
            { "label" , "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label" , "Output" },
            { "icon", "⚡" },
            { "value", electricityOutput.ToString() }
        }
    };
    public string GetDescription() => description;
}
