using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour, IBuilding , IBuildingUiData, ISellable
{

    // dont need canPlace in Ibuilding you can get it true IbuildingSo which is in all buildings
    CityData cityData;
    [SerializeField] public HousesSO houseSO;
    //[SerializeField] Image elecMissingImage;
    //[SerializeField] TMP_Text label;

    // Buildings that depend on it and how man workers they receive from this script
    Dictionary<IBuilding, int> workerDependants = new();

    int electricityReceived = 0;
    int workersGiven = 0;

    bool isActive = false;

    void Start()
    {
        //elecMissingImage.color = Color.blueViolet;
        cityData.addMoney(-houseSO.price);
        cityData.addPopulation(houseSO.population);
        cityData.useWater(houseSO.waterNeeded);

        cityData.electricityDependants.Add(this);
        cityData.workerProviders.Add(this);
        cityData.ElectrifyBuildings();
        cityData.EmployWorkers();


    }

    public void SetCityData(CityData cityData)
    {
        this.cityData = cityData;
    }
    public void EmployWorkers()
    {
        if (!isActive)
            return;

        List<IBuilding> buildingsToRemove = new();

        foreach (var dependant in cityData.workerDependants)
        {
            if (workerDependants.ContainsKey(dependant))
                continue;

            if (!HasEnoughWorkers())
            {
                cityData.workerProviders.Remove(this);
                Debug.Log("No more workers left");
                break;
            }

            int workersAvailable = houseSO.population - workersGiven;
            int workersNeeded = dependant.GetWorkersNeeded();
            int amountToGive = Mathf.Min(workersAvailable, workersNeeded);

            dependant.ReceiveWorkers(amountToGive);
            workersGiven += amountToGive;
            workerDependants.Add(dependant, amountToGive);

            Debug.Log("Got here");

            if (dependant.HasEnoughWorkers())
            {
                dependant.Activate();
                buildingsToRemove.Add(dependant);
            }
        }

        foreach (IBuilding building in buildingsToRemove)
        {
            cityData.workerDependants.Remove(building);
        }
    }
    public void Sell()
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        cityData.addMoney(houseSO.price * .8f);
        // ! Might change this in the future to the exact same system as the electricity
        cityData.addPopulation(-houseSO.population);
        cityData.UseElectricity(-houseSO.electricityNeeded);
        cityData.useWater(-houseSO.waterNeeded);

        foreach (var (dependant, amountGiven) in workerDependants)
        {
            dependant.Deactivate(0, amountGiven);
            if (!cityData.workerDependants.Contains(dependant))
                cityData.workerDependants.Add(dependant);
        }
    }
    public void Deactivate(int electricityLost, int workersLost)
    {
        electricityReceived -= electricityLost;
        cityData.addWorkingPopulation(-houseSO.population);

        foreach (var (dependant, amountReceived) in workerDependants)
        {
            dependant.Deactivate(0, amountReceived);
            if (!cityData.workerDependants.Contains(dependant))
                cityData.workerDependants.Add(dependant);
        }
    }
    public void Activate()
    {
        isActive = true;
        cityData.addWorkingPopulation(houseSO.population);
        EmployWorkers();
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        cityData.UseElectricity(electricity);
    }

    public IBuildingSO GetBuildingSO() => houseSO;
    public string GetBuildingType() => houseSO.type;
    public string GetRessource() => houseSO.ressource;
    public int GetPopulation() => houseSO.population;
    public int GetPrice() => houseSO.price;
    public int GetElectricityReceived() => electricityReceived;
    public int GetElectricityNeeded() => houseSO.electricityNeeded - electricityReceived;
    public bool CanPlace() => cityData.getMoney >= houseSO.price;
    public bool IsActive() => isActive;
    public void SetActive() {
        isActive = true;
        cityData.addPopulation(houseSO.population);
    }
    public void Deactivate()
    {
        isActive = false;
        cityData.addPopulation(-houseSO.population);
        
    }

    public List<UiDataText> GetUiTextList()
    {
        
        return new List<UiDataText>
        { 
           new UiDataText(UiDataType.NAME, "House"),
           new UiDataText(UiDataType.PRICE, "Current price : " + houseSO.price.ToString()),
        };
    }

    public List<UiDataInteraction> GetUiInteractionList()
    {
        return new List<UiDataInteraction>
        {
            new UiDataInteraction(UiInteractionType.SELL, new Sell())
        };
    }
    public bool HasEnoughElectricity() => electricityReceived >= houseSO.electricityNeeded;
    public bool HasEnoughWorkers() => workersGiven < houseSO.population;
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;

    
}