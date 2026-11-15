using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInfo
{
    public static PlayerInfo Instance { get; } = new PlayerInfo();

    int population = 0;
    int workingPopulation = 0;
    int electricityTotal = 0;
    int electricityUsed = 0;
    int waterTotal = 0;
    int waterUsed = 0;
    float money = 1000;
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

    private PlayerInfo() { InitializeResources(); }
    private const int MoneyPerExtractedResource = 10;

    public Dictionary<ResourceType, int> resources = new();
    private Dictionary<ResourceType, int> exploitedResources = new();

    private void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            resources[type] = 0;
            exploitedResources[type] = 0;
        }
    }

    public void AddPopulation(int population)
    {
        this.population += population;
        addStat?.Invoke("PopulationValue", $"{population}");
        if (population <= 0) workingPopulation = Mathf.Min(workingPopulation, population);
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

<<<<<<< HEAD
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
        this.money += money;
        addStat?.Invoke("MoneyValue", $"{money}");
    }
    public float GetMoney => money;

    public void AddWood(int wood) { resources[ResourceType.Wood] += wood; }
    public int GetWood => resources[ResourceType.Wood];

    public void AddStone(int stone) { resources[ResourceType.Stone] += stone; }
    public int GetStone => resources[ResourceType.Stone];

    public void AddResource(ResourceType type, int amount) { resources[type] += amount; }

    public float GetResearchPoints() => researchPoints;
    public void AddResearchPoints(float points)
    {
        researchPoints += points;
        addStat?.Invoke("ResearchPointsValue", $"{points}");
=======
    public void addWater(int water) { this.water += water; }
    public void useWater(int water) { this.waterUsed += water; }
    public int getWater => water;
    public int getWaterUsed => waterUsed;
    public int getAvailableWater => water - waterUsed;

    public void addFood(int food) { this.food += food; }
    public int getFood => food;

    public void addMoney(float money) { this.money += money; }
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
>>>>>>> c75f9cff08f3bdfc6c5651a1eb04552ba1965b7c
    }

    public void StartProvidingElectricity()
    {
        List<Electricity> providersToRemove = new();
        foreach (var provider in electricityProviders)
        {
            if (electricityDependants.Count < 1) break;
            provider.ProvideElectricity();
            if (!provider.HasAvailableElectricity()) providersToRemove.Add(provider);
        }
        foreach (Electricity provider in providersToRemove) electricityProviders.Remove(provider);
    }

    public void StartProvidingWorkers()
    {
        List<House> providersToRemove = new();
        foreach (var provider in workerProviders)
        {
            if (workerDependants.Count < 1) break;
            provider.ProvideWorkers();
            if (!provider.HasEnoughWorkers()) providersToRemove.Add(provider);
        }
        foreach (var provider in providersToRemove) workerProviders.Remove(provider);
    }

    public void StartProvidingWater()
    {
        List<Water> providersToRemove = new();
        foreach (Water provider in waterProviders)
        {
            if (waterDependants.Count < 1) break;
            provider.ProvideWater();
            if (!provider.HasAvailableWater()) providersToRemove.Add(provider);
        }
        foreach (Water provider in providersToRemove) waterProviders.Remove(provider);
    }
}
