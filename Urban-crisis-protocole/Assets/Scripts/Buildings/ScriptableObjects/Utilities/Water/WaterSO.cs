using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Water", menuName = "ScriptableObjects/Water")]
public class WaterSO : ScriptableObject, IBuildingSO
{
    // Identity & Metadata
    [SerializeField] string buildingName;
    [SerializeField] public string description;
    [SerializeField] Sprite icon;
    [SerializeField] BuildingCard card;

    // Classification
    [SerializeField] string type = "Utilities";
    [SerializeField] string ressource = "Water";
    [SerializeField] BuildingClasses buildingClass = BuildingClasses.Water;
    [SerializeField] BuildingTypes buildingType;

    // Economics & Costs
    [SerializeField] int price = 25;
    [SerializeField] int water = 10;
    [SerializeField] int researchPrice;
    [SerializeField] float researchOutput;

    // Requirements & Operations
    [SerializeField] int electricityNeeded = 0;

    // Interface Implementation
    public string GetBuildingName() => "💧 " + buildingName;
    public string GetDescription() => description;
    public int GetPrice() => price;
    public int GetWaterOutput() => water;
    public int GetElectricityNeeded() => electricityNeeded;
    public string GetRessource() => ressource;
    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public BuildingTypes GetBuildingType() => buildingType;
    public int GetResearchPrice() => researchPrice;
    public float GetResearchOutput() => researchOutput;

    public bool CanPlace() => price <= PlayerInfo.Instance.GetMoney;

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>> {
        new Dictionary<string, string> {
            { "label", "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Output" },
            { "icon", "💧" },
            { "value", water.ToString() }
        }
    };
}