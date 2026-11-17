using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "House", menuName = ("ScriptableObjects/House"))]
public class HousesSO : ScriptableObject, IBuildingSO
{
    // Identity & Metadata
    [SerializeField] string buildingName;
    [SerializeField] public string description;
    [SerializeField] Sprite icon;
    [SerializeField] BuildingCard card;

    // Classification
    [SerializeField] string type = "Population";
    [SerializeField] string ressource = "Population";
    [SerializeField] BuildingClasses buildingClass = BuildingClasses.House;
    [SerializeField] BuildingTypes buildingType;

    // Economics & Costs
    [SerializeField] int price;
    [SerializeField] int population;
    [SerializeField] int researchPrice;
    [SerializeField] float researchOutput;

    // Requirements & Operations
    [SerializeField] int electricityNeeded;
    [SerializeField] int waterNeeded;

    // Interface Implementation
    public string GetBuildingName() => "🏡 " + buildingName;
    public string GetDescription() => description;
    public int GetPrice() => price;
    public int GetPopulation() => population;
    public int GetElectricityNeeded() => electricityNeeded;
    public int GetWaterNeeded() => waterNeeded;
    public string GetRessource() => ressource;
    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public BuildingTypes GetBuildingType() => buildingType;
    public int GetResearchPrice() => researchPrice;
    public float GetResearchOutput() => researchOutput;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    public bool CanPlace() =>
        playerInfo.GetMoney >= price &&
        playerInfo.GetAvailableElectricity >= electricityNeeded;  // !! water 

    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>>
    {
        new Dictionary<string, string> {
            { "label", "Price" },
            { "icon", "💵" },
            { "value", price.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Population" },
            { "icon", "🏡" },
            { "value", population.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Consumes" },
            { "icon", "⚡" },
            { "value", electricityNeeded.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Consumes" },
            { "icon", "💧" },
            { "value", waterNeeded.ToString() }
        }
    };
}