using UnityEngine;
using System.Collections.Generic;
using System;

public partial class CityData
{


    int population = 0;
    int workingPopulation = 0;
    int electricityTotal = 0;
    int electricityUsed = 0;
    int waterTotal = 0;
    int waterUsed = 0;

    float money = 2000;
    float researchPoints = 20;

    public Dictionary<ResourceType, int> resources = new();
    public List<IBuilding> electricityDependants = new();
    public List<Electricity> electricityProviders = new();
    public List<House> workerProviders = new();
    public List<IBuilding> workerDependants = new();
    public List<Water> waterProviders = new();
    public List<House> waterDependants = new();

    public List<BuildingTypes> housesUnlocked = new() { BuildingTypes.SmallHouse };
    public List<BuildingTypes> businessesUnlocked = new() { BuildingTypes.GasStation };
    public List<BuildingTypes> electricityUnlocked = new() { BuildingTypes.SolarPanels };
    public List<BuildingTypes> waterUnlocked = new() { BuildingTypes.WaterSilo };
    public List<BuildingTypes> militaryUnlocked = new() { BuildingTypes.Tent };

    public event Action<string, string> addStat;

    public CityData() { InitializeResources(); }
    private const int MoneyPerExtractedResource = 10;

  
    private Dictionary<ResourceType, int> exploitedResources = new();

    private void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
            exploitedResources[type] = 0;
        }
    }

    public void AddPopulation(int population)
    {
        this.population += population;
        addStat?.Invoke("PopulationValue", $"{population}");
        if (population <= 0) workingPopulation = Mathf.Min(workingPopulation, this.population);
    }
    public int GetPopulation => population;
    public void AddWorkingPopulation(int population) { this.workingPopulation += population; }
    public int GetWorkingPopulation => workingPopulation;
    public int GetAvailablePopulation => population - workingPopulation;

    public void AddElectricity(int electricity)
    {
        electricityTotal += electricity;
        addStat?.Invoke("ElectricityValue", $"{electricity}");
    }
    public void UseElectricity(int electricity) { electricityUsed += electricity; }
    public int GetElectricity => electricityTotal;
    public int GetElectricityUsed => electricityUsed;
    public int GetAvailableElectricity => electricityTotal - electricityUsed;

    public void AddWater(int water)
    {
        waterTotal += water;
        addStat?.Invoke("WaterValue", $"{water}");
    }
    public void UseWater(int water) { waterUsed += water; }
    public int GetWater => waterTotal;
    public int GetWaterUsed => waterUsed;
    public int GetAvailableWater => waterTotal - waterUsed;


    public void AddMoney(float money)
    {
        this.money = (float)Math.Round((double)this.money + money, 2, MidpointRounding.AwayFromZero);
        addStat?.Invoke("MoneyValue", $"{money}");
    }
    public bool TrySpend(float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0 || money < amount) return false;
        AddMoney(-amount);
        return true;
    }
    public float GetMoney => money;
    public string FormattedMoney => money.ToString("N2");

    public void AddWood(int wood) { addResource(ResourceType.Wood, wood); }
    public int GetWood => resources[ResourceType.Wood];

    public void AddStone(int stone) { addResource(ResourceType.Stone, stone); }
    public int GetStone => resources[ResourceType.Stone];

    public void AddResource(ResourceType type, int amount) { addResource(type, amount); }

    public float GetResearchPoints() => researchPoints;
    public void AddResearchPoints(float points)
    {
        researchPoints += points;
        addStat?.Invoke("ResearchPointsValue", $"{points}");
    }
    public void addWater(int water) { AddWater(water); }
    public void useWater(int water) { this.waterUsed += water; }
    public int getWater => waterTotal;
    public int getWaterUsed => waterUsed;
    public int getAvailableWater => waterTotal - waterUsed;

    // public void addFood(int food) { this.food += food; }
    // public int getFood => food;

    public void addMoney(float money) { AddMoney(money); }
    public float getMoney => money;

    public void addWood(int wood) { addResource(ResourceType.Wood, wood); }
    public int getWood => resources[ResourceType.Wood];

    public void addStone(int stone) { addResource(ResourceType.Stone, stone); }
    public int getStone => resources[ResourceType.Stone];

    public int getCoal => resources[ResourceType.Coal];

    public int getExploitedWood => exploitedResources[ResourceType.Wood];
    public int getExploitedStone => exploitedResources[ResourceType.Stone];
    public int getExploitedCoal => exploitedResources[ResourceType.Coal];
    public int getTotalExploited =>
        getExploitedWood + getExploitedStone + getExploitedCoal;

    public void addResource(ResourceType type, int amount)
    {
        resources[type] += amount;

        if (amount > 0)
        {
            exploitedResources[type] += amount;
        }
    }

    // Money is awarded when a resource is extracted; stock may arrive later.
    public void rewardExtraction(int amount)
    {
        if (amount > 0) addMoney(amount * MoneyPerExtractedResource);
    }

    public void collectResource(ResourceType type, int amount)
    {
        if (amount <= 0) return;
        addResource(type, amount);
        rewardExtraction(amount);
    }

    public void StartProvidingElectricity() => RecalculateBuildings();
    public void StartProvidingWorkers() => RecalculateBuildings();
    public void StartProvidingWater() => RecalculateBuildings();

    private int food;
    private readonly Dictionary<GameObject, List<Vector3>> buildingsPositions = new();
    public List<Vector3> GetAllPositionStored()
    {
        var result = new List<Vector3>();
        var removed = new List<GameObject>();
        foreach (var entry in buildingsPositions)
        {
            if (entry.Key == null) { removed.Add(entry.Key); continue; }
            result.AddRange(entry.Value);
        }
        foreach (var item in removed) buildingsPositions.Remove(item);
        return result;
    }
    public void StoreNewBuilding(GameObject building, List<Vector3> occupied)
    {
        buildingsPositions[building] = new List<Vector3>(occupied);
    }
    public void RemoveBuilding(GameObject building) => buildingsPositions.Remove(building);
    public void addPopulation(int value) => AddPopulation(value);
    public int getPopulation => GetPopulation;
    public void addWorkingPopulation(int value) => AddWorkingPopulation(value);
    public int getWorkingPopulation => GetWorkingPopulation;
    public int getAvailablePopulation => GetAvailablePopulation;
    public void addElectricity(int value) => AddElectricity(value);
    public int getElectricity => GetElectricity;
    public int getElectricityUsed => GetElectricityUsed;
    public int getAvailableElectricity => GetAvailableElectricity;
    public void addFood(int value) => food += value;
    public int getFood => food;
    public void ElectrifyBuildings() => StartProvidingElectricity();
    public void EmployWorkers() => StartProvidingWorkers();

}
