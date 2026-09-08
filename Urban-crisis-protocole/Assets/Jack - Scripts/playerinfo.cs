using UnityEngine;
using System.Collections.Generic;

public class PlayerInfo
{
    public static PlayerInfo Instance { get; } = new PlayerInfo();

    private PlayerInfo() { }

    int workingPopulation = 0;
    int population = 0;
    int electricity = 0;
    int electricityUsed = 0;
    int water = 0;
    int waterUsed;
    int food = 0;
    float money = 2000;

    public void addPopulation(int population) { this.population += population; }
    public int getPopulation => population;
    public void addWorkingPopulation(int population) { this.population += population; }
    public int getWorkingPopulation => workingPopulation;
    public int getAvailablePopulation => population - workingPopulation;
    public void addElectricity(int electricity) { this.electricity += electricity; }
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

    List<GameObject> electricityQueue;
}