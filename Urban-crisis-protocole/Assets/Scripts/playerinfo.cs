using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInfo
{
    public static PlayerInfo Instance { get; } = new PlayerInfo();

    private PlayerInfo()
    {
        InitializeResources();
    }

    int workingPopulation = 0;
    int population = 0;
    int electricity = 0;
    int electricityUsed = 0;
    int water = 0;
    int waterUsed;
    int food = 0;
    float money = 2000;

    public Dictionary<ResourceType, int> resources = new();

    private void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
    }

    public void addPopulation(int population)
    {
        if (population > 0)
        {
            this.population += population;
        }
        else
        {
            this.population += population;
            workingPopulation = Mathf.Min(workingPopulation, population);
        }
    }
    public int getPopulation => population;
    public void addWorkingPopulation(int population) { this.workingPopulation += population; }
    public int getWorkingPopulation => workingPopulation;
    public int getAvailablePopulation => population - workingPopulation;

    public void addElectricity(int electricity)
    {
        if (electricity > 0)
        {
            this.electricity += electricity;
        }
        else
        {
            this.electricity += electricity;
            electricityUsed = Mathf.Min(electricityUsed, electricity);
        }
    }
    public int getElectricity => electricity;
    public void UseElectricity(int electricity) { this.electricityUsed += electricity; }
    public int getElectricityUsed => electricityUsed;
    public int getAvailableElectricity => electricity - electricityUsed;

    public void addWater(int water) { this.water += water; }
    public void useWater(int water) { this.waterUsed += water; }
    public int getWater => water;
    public int getWaterUsed => waterUsed;
    public int getAvailableWater => water - waterUsed;

    public void addFood(int food) { this.food += food; }
    public int getFood => food;

    public void addMoney(float money) { this.money += money; }
    public float getMoney => money;

    public void addWood(int wood) { resources[ResourceType.Wood] += wood; }
    public int getWood => resources[ResourceType.Wood];

    public void addStone(int stone) { resources[ResourceType.Stone] += stone; }
    public int getStone => resources[ResourceType.Stone];

    public void addResource(ResourceType type, int amount)
    {
        resources[type] += amount;
    }

    public List<GameObject> electricityList = new List<GameObject>();

    public void ElectrifyBuildings(ElectricitySO electricitySO)
    {
        if (electricityList.Count > 0)
        {
            electricitySO.AddElectricity();

            foreach (GameObject go in electricityList)
            {
                if (getAvailableElectricity == 0)
                    break;

                if (getAvailableElectricity >= go.GetComponent<IBuilding>().GetElectricityNeeded())
                {
                    UseElectricity(go.GetComponent<IBuilding>().GetElectricityNeeded());
                    electricityList.Remove(go);
                }
            }
        }
    }
}