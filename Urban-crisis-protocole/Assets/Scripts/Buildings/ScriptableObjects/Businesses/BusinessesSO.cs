using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Business", menuName = "ScriptableObjects/Business")]
public class BusinessesSO : ScriptableObject, IBuildingSO
{
    // Identity & Metadata
    [SerializeField] string buildingName;
    [SerializeField] public string description;
    [SerializeField] Sprite icon;
    [SerializeField] BuildingCard card;

    // Classification
    [SerializeField] string type = "Business";
    [SerializeField] string ressource = "Money";
    [SerializeField] BuildingClasses buildingClass = BuildingClasses.Business;
    [SerializeField] BuildingTypes buildingType;

    // Economics & Costs
    [SerializeField] int price;
    [SerializeField] int profit;
    [SerializeField] int researchPrice;
    [SerializeField] float researchOutput;

    // Requirements & Operations
    [SerializeField] int electricityNeeded;
    [SerializeField] int workersNeeded;

    // Interface Implementation
    public string GetBuildingName() => "💵 " + buildingName;
    public string GetDescription() => description;
    public int GetPrice() => price;
    public int GetProfit() => profit;
    public int GetElectricityNeeded() => electricityNeeded;
    public string GetRessource() => ressource;
    public int GetWorkersNeeded() => workersNeeded;
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
            { "label", "Profit" },
            { "icon", "💵" },
            { "value", profit.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Workers" },
            { "icon", "👥" },
            { "value", workersNeeded.ToString() }
        },
        new Dictionary<string, string> {
            { "label", "Consumes" },
            { "icon", "⚡" },
            { "value", electricityNeeded.ToString() }
        }
    };
}