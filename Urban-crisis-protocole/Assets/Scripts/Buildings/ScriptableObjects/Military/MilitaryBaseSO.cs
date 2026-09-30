using UnityEngine;

[CreateAssetMenu(fileName = "Military Base", menuName = "ScriptableObjects/Military Base")]
public class MilitaryBaseSO : ScriptableObject, IBuildingSO
{
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
}
