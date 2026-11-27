using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Water Silo", menuName = "ScriptableObjects/Hut Station")]
public class HutStationsSO : ScriptableObject, IBuildingSO
{
    public string type = "Utilities";
    public string resource;
    public int electricityNeeded = 0;
    public int population;
    public int resourceAmount = 1;
    public int price = 25;

    [SerializeField] private BuildingCard card;
    public Sprite GetIcon() => null;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.GetMoney;
    public string GetBuildingName() => name;
    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>>
    {
        new Dictionary<string, string> { { "label", "Price" }, { "icon", "$" }, { "value", price.ToString() } },
        new Dictionary<string, string> { { "label", "Resource" }, { "icon", "" }, { "value", resource } },
        new Dictionary<string, string> { { "label", "Workers" }, { "icon", "" }, { "value", population.ToString() } },
        new Dictionary<string, string> { { "label", "Power" }, { "icon", "" }, { "value", electricityNeeded.ToString() } }
    };
    public string GetDescription() => "Collects nearby " + resource + " while supplied.";
    public BuildingTypes GetBuildingType() => resource != null && resource.Equals("stone", System.StringComparison.OrdinalIgnoreCase) ? BuildingTypes.StoneHut : BuildingTypes.WoodHut;
    public int GetResearchPrice() => 0;
}
