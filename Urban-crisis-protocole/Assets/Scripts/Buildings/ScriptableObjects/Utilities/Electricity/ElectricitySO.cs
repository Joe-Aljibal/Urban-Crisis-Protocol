using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ElectricitySO", menuName = "ScriptableObjects/ElectricitySO")]
public class ElectricitySO : ScriptableObject, IBuildingSO
{
    // Identity & Metadata
    [SerializeField] string buildingName;
    [SerializeField] public string description;
    [SerializeField] Sprite icon;
    [SerializeField] BuildingCard card;

    // Classification
    [SerializeField] string type = "Utility";
    [SerializeField] string ressource = "Electricity";
    [SerializeField] BuildingClasses buildingClass = BuildingClasses.Electricity;
    [SerializeField] BuildingTypes buildingType;

    // Economics & Costs
    [SerializeField] int price;
    [SerializeField] int electricityOutput;
    [SerializeField] int researchPrice;
    [SerializeField] float researchOutput;

    // Interface Implementation
    public string GetBuildingName() => "⚡ " + buildingName;
    public string GetDescription() => description;
    public int GetPrice() => price;
    public int GetElectricityOutput() => electricityOutput;
    public string GetRessource() => ressource;
    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public BuildingTypes GetBuildingType() => buildingType;
    public int GetResearchPrice() => researchPrice;
    public float GetResearchOutput() => researchOutput;

    public bool CanPlace() => price <= PlayerInfo.Instance.GetMoney;

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>>
    {
        new Dictionary<string, string> {
            { "label", "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Output" },
            { "icon", "⚡" },
            { "value", electricityOutput.ToString() }
        }
    };
}