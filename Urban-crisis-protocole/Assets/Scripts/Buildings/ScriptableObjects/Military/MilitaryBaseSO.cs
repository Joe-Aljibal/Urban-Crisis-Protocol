using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Military Base", menuName = "ScriptableObjects/Military Base")]
public class MilitaryBaseSO : ScriptableObject, IBuildingSO
{
    [SerializeField] Sprite icon;
    [SerializeField] BuildingCard card;
    [SerializeField] string description = "Military defense and soldier patrols.";
    [SerializeField] int researchPrice;
    public string type = "Military";
    public string ressource = "Defense";
    public int electricityNeeded = 150;
    public int population = 12;       // soldats
    public int price = 4000;
    public float upkeep = 20;         // solde des soldats, par seconde
    [Range(0, 1)]
    public float interceptChance = 0.6f; // chance d'arreter chaque bandit

    public bool CanPlace() =>
        price <= PlayerInfo.Instance.getMoney &&
        electricityNeeded <= PlayerInfo.Instance.getAvailableElectricity &&
        population <= PlayerInfo.Instance.getAvailablePopulation;

    public BuildingTypes GetBuildingType() => BuildingTypes.Tent;
    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public string GetBuildingName() => "Military Base";
    public string GetDescription() => description;
    public int GetResearchPrice() => researchPrice;
    public List<Dictionary<string, string>> GetStats() => new List<Dictionary<string, string>> {
        new Dictionary<string, string> { { "label", "Price" }, { "icon", "$" }, { "value", price.ToString() } },
        new Dictionary<string, string> { { "label", "Soldiers" }, { "icon", "" }, { "value", population.ToString() } }
    };
}
