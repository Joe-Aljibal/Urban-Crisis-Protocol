using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch-mode integration check: Unity.exe -batchmode -projectPath ... -executeMethod CityEconomyCheck.Run
[InitializeOnLoad]
public static class CityEconomyCheck
{
    const string Running = "CityEconomyCheck.Running";
    static int step;
    static House house; static Electricity power; static Water water; static Business business; static MilitaryBase military; static GambleBuilding gamble;
    static CityPowerOutage outage;
    static CityData city;
    static CityEconomyCheck() { EditorApplication.update += Update; }
    public static void Run()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }
    static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(obj, value);
    static T Add<T>(string name, ScriptableObject data, string field) where T : MonoBehaviour
    {
        var go = new GameObject(name);
        var component = go.AddComponent<T>(); Set(component, field, data);
        var structure = go.AddComponent<CityStructure>(); structure.Configure("Construction", Color.white);
        return component;
    }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static void CheckProductionRequirements()
    {
        var owner = new CityData();
        var config = ScriptableObject.CreateInstance<HutStationsSO>();
        config.electricityNeeded = 10; config.population = 2;
        var producer = Add<HutStation>("Producer requirements", config, "woodHutStationSO");
        producer.SetCityData(owner); owner.RegisterBuilding(producer);
        Check(!producer.IsActive(), "Production must wait for power and workers.");
        var powerData = ScriptableObject.CreateInstance<ElectricitySO>(); Set(powerData, "electricityOutput", 10);
        var plant = Add<Electricity>("Producer power", powerData, "electricitySO");
        plant.SetCityData(owner); owner.RegisterBuilding(plant);
        Check(!producer.IsActive(), "Power alone must not replace required workers.");
        var housingData = ScriptableObject.CreateInstance<HousesSO>(); Set(housingData, "population", 5);
        var housing = Add<House>("Producer housing", housingData, "houseSO");
        housing.SetCityData(owner); owner.RegisterBuilding(housing);
        Check(producer.IsActive() && owner.GetWorkingPopulation == 2, "Power and workers must activate production.");
        owner.SetPowerOutage(true); Check(!producer.IsActive(), "An outage must stop production.");
        owner.SetPowerOutage(false); Check(producer.IsActive(), "Restoring power must resume supplied production.");
        owner.SetBuildingAvailable(housing, false); Check(!producer.IsActive(), "Losing workers must stop production.");
    }
    static void Update()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
        try
        {
            if (step == 0)
            {
                Cities.ResetForNewGame(); city = Cities.cities["PLAYER"];
                var p = ScriptableObject.CreateInstance<ElectricitySO>(); Set(p, "electricityOutput", 100); power = Add<Electricity>("Power", p, "electricitySO");
                var w = ScriptableObject.CreateInstance<WaterSO>(); Set(w, "water", 30); Set(w, "electricityNeeded", 10); water = Add<Water>("Water", w, "waterSO");
                var h = ScriptableObject.CreateInstance<HousesSO>(); Set(h, "population", 20); Set(h, "electricityNeeded", 10); Set(h, "waterNeeded", 10); house = Add<House>("House", h, "houseSO");
                var b = ScriptableObject.CreateInstance<BusinessesSO>(); Set(b, "electricityNeeded", 10); Set(b, "workersNeeded", 5); business = Add<Business>("Business", b, "businessSO");
                var g = ScriptableObject.CreateInstance<GambleBuildingSO>(); g.electricityNeeded = 5; g.workersNeeded = 2; gamble = Add<GambleBuilding>("Casino", g, "gambleBuildingSO");
                var m = ScriptableObject.CreateInstance<MilitaryBaseSO>(); m.population = 5; m.electricityNeeded = 10; military = Add<MilitaryBase>("Military", m, "militaryBaseSO");
                outage = new GameObject("Outage").AddComponent<CityPowerOutage>();
                step++; return;
            }
            if (step == 1)
            {
                Check(city.GetPopulation == 20 && city.GetElectricity == 100 && city.GetElectricityUsed == 45 && city.GetWater == 30 && city.GetWaterUsed == 10 && city.GetWorkingPopulation == 12, "Initial supplies and workers must be allocated once.");
                Check(house.IsActive() && business.IsActive() && gamble.IsActive() && military.IsActive(), "Supplied buildings must operate.");
                water.GetComponent<CityStructure>().DestroyObject();
                Check(city.GetWater == 0 && city.GetWorkingPopulation == 0 && !business.IsActive(), "Destroying water must interrupt houses and workers.");
                water.GetComponent<CityStructure>().Repair();
                Check(city.GetWater == 30 && city.GetWorkingPopulation == 12, "Repair must restore supplies once.");
                water.GetComponent<CityStructure>().Repair();
                Check(city.GetWater == 30 && city.GetWorkingPopulation == 12, "Repeated repairs must not duplicate supplies.");
                house.GetComponent<CityStructure>().DestroyObject();
                Check(city.GetPopulation == 0 && city.GetWorkingPopulation == 0, "Destroyed houses must release residents and jobs.");
                outage.TriggerOutage(); house.GetComponent<CityStructure>().Repair();
                Check(!house.IsActive() && !house.enabled && city.GetElectricity == 0, "Repair during an outage must not restore power.");
                Check(outage.DispatchRepair(), "Outage must be repairable.");
                Check(house.enabled && house.IsActive() && city.GetWorkingPopulation == 12, "Outage repair must restore a repaired house.");
                water.Deactivate(); Check(city.GetWater == 0 && city.GetWorkingPopulation == 0, "Disabling a water plant must release water and jobs.");
                water.Activate(); Check(city.GetWater == 30 && city.GetWorkingPopulation == 12, "Re-enabling a water plant must restore supply once.");
                business.Sell(); Check(city.GetWorkingPopulation == 7, "Selling a business must release its workers.");
                military.Sell(); Check(city.GetWorkingPopulation == 2 && MilitaryBase.ActiveBaseCount == 0, "Selling a military base must release soldiers and defense.");
                gamble.Sell(); Check(city.GetWorkingPopulation == 0, "Selling a casino must release its workers.");
                water.Sell(); Check(city.GetWater == 0 && city.GetWaterUsed == 0, "Selling water infrastructure must remove its supply.");
                house.Sell(); Check(city.GetPopulation == 0, "Selling a house must remove its population.");
                power.Sell(); Check(city.GetElectricity == 0 && city.GetElectricityUsed == 0, "Selling the power plant must remove supply.");
                TownResourceStatistics.AddResource(TownType.Human, ResourceType.Wood, 3);
                TownResourceStatistics.AddResource(TownType.AI, ResourceType.Stone, 2);
                Check(city.GetWood == 3 && TownResourceStatistics.GetWood(TownType.Human) == 3 && Cities.cities["AI"].GetStone == 2, "Resource counters must share city data.");
                city.AddWood(2); city.AddResource(ResourceType.Stone, 1);
                Check(TownResourceStatistics.GetWood(TownType.Human) == 5 && TownResourceStatistics.GetStone(TownType.Human) == 1, "All resource entry points must update the same statistics.");
                city.AddMoney(100); city.housesUnlocked.Add(BuildingTypes.MediumHouse);
                Cities.ResetForNewGame();
                Check(city.GetMoney == 2000 && city.GetResearchPoints() == 20 && city.GetWood == 0 && city.GetPopulation == 0 && !city.housesUnlocked.Contains(BuildingTypes.MediumHouse) && TownResourceStatistics.GetStone(TownType.AI) == 0, "New game must reset both cities in place.");
                CheckProductionRequirements();
                SessionState.SetBool(Running, false); Debug.Log("CITY_ECONOMY_CHECK_PASSED"); EditorApplication.Exit(0);
            }
        }
        catch (Exception ex) { SessionState.SetBool(Running, false); Debug.LogException(ex); EditorApplication.Exit(1); }
    }
}
