using System.Collections.Generic;
using UnityEngine;

// All supplies are recomputed together, in placement order, whenever the city changes.
public partial class CityData
{
    private readonly List<IBuilding> buildings = new();
    private readonly HashSet<IBuilding> suspendedBuildings = new();
    private bool powerOutage;

    public void RegisterBuilding(IBuilding building)
    {
        if (!buildings.Contains(building)) buildings.Add(building);
        RecalculateBuildings();
    }
    public void UnregisterBuilding(IBuilding building)
    {
        buildings.Remove(building);
        suspendedBuildings.Remove(building);
        RecalculateBuildings();
    }
    public void SetBuildingAvailable(IBuilding building, bool available)
    {
        if (available) suspendedBuildings.Remove(building); else suspendedBuildings.Add(building);
        RecalculateBuildings();
    }
    public void SetPowerOutage(bool value) { powerOutage = value; RecalculateBuildings(); }
    public void ResetForNewGame()
    {
        buildings.Clear(); suspendedBuildings.Clear(); buildingsPositions.Clear();
        electricityDependants.Clear(); electricityProviders.Clear(); workerProviders.Clear();
        workerDependants.Clear(); waterProviders.Clear(); waterDependants.Clear();
        population = workingPopulation = electricityTotal = electricityUsed = waterTotal = waterUsed = food = 0;
        money = 2000; researchPoints = 20; powerOutage = false;
        InitializeResources();
        housesUnlocked.Clear(); housesUnlocked.Add(BuildingTypes.SmallHouse);
        businessesUnlocked.Clear(); businessesUnlocked.Add(BuildingTypes.GasStation);
        electricityUnlocked.Clear(); electricityUnlocked.Add(BuildingTypes.SolarPanels);
        waterUnlocked.Clear(); waterUnlocked.Add(BuildingTypes.WaterSilo);
        militaryUnlocked.Clear(); militaryUnlocked.Add(BuildingTypes.Tent);
    }
    private bool Available(IBuilding building)
    {
        var component = building as Component;
        if (component == null || suspendedBuildings.Contains(building)) return false;
        var condition = component.GetComponent<CityStructure>();
        return condition == null || !condition.IsDestroyed;
    }
    private static int Allocate(ref int available, int needed)
    {
        int amount = Mathf.Min(available, Mathf.Max(0, needed));
        available -= amount;
        return amount;
    }
    public void RecalculateBuildings()
    {
        buildings.RemoveAll(b => (b as Component) == null);
        population = workingPopulation = electricityTotal = electricityUsed = waterTotal = waterUsed = 0;
        foreach (var b in buildings)
        {
            if (!Available(b)) continue;
            if (b is Electricity e && !powerOutage) electricityTotal += e.Output;
            if (b is House h) population += h.GetPopulation();
        }
        int power = electricityTotal;
        // Water infrastructure gets power first so houses can receive water.
        foreach (var b in buildings)
            if (b is Water w)
            {
                int supplied = Available(b) && !powerOutage ? Allocate(ref power, w.PowerDemand) : 0;
                w.ApplySupply(supplied, Available(b) && !powerOutage);
                if (w.IsActive()) waterTotal += w.GetWaterOutput();
            }
        var powers = new Dictionary<IBuilding, int>();
        foreach (var b in buildings)
        {
            if (b is Water || b is Electricity) continue;
            int demand = b is House h ? h.PowerDemand : b is Business business ? business.PowerDemand : b is GambleBuilding g ? g.PowerDemand : b is MilitaryBase m ? m.PowerDemand : 0;
            powers[b] = Available(b) && !powerOutage ? Allocate(ref power, demand) : 0;
        }
        electricityUsed = electricityTotal - power;
        int water = waterTotal;
        int workers = 0;
        foreach (var b in buildings)
            if (b is House h)
            {
                bool available = Available(b) && !powerOutage;
                int supplied = available ? Allocate(ref water, h.WaterDemand) : 0;
                h.ApplySupply(powers[b], supplied, Available(b), available);
                if (h.IsActive()) workers += h.GetPopulation();
            }
        waterUsed = waterTotal - water;
        foreach (var b in buildings)
        {
            bool available = Available(b) && !powerOutage;
            if (b is Business business)
            {
                int assigned = available && powers[b] >= business.PowerDemand ? Allocate(ref workers, business.WorkerDemand) : 0;
                business.ApplySupply(powers[b], assigned, available); workingPopulation += assigned;
            }
            else if (b is GambleBuilding gamble)
            {
                int assigned = available && powers[b] >= gamble.PowerDemand ? Allocate(ref workers, gamble.WorkerDemand) : 0;
                gamble.ApplySupply(powers[b], assigned, available); workingPopulation += assigned;
            }
            else if (b is MilitaryBase military)
            {
                int assigned = available && powers[b] >= military.PowerDemand ? Allocate(ref workers, military.WorkerDemand) : 0;
                military.ApplySupply(powers[b], assigned, available); workingPopulation += assigned;
            }
            else if (b is Electricity e) e.ApplySupply(available);
        }
    }
}
