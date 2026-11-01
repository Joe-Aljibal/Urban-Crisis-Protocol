using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour, IBuilding, IWorkerProvider, IBuildingUiData
{

    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] public HousesSO houseSO;

    // Buildings that depend on it and how man workers they receive from this script
    Dictionary<IBuilding, int> workerDependants = new();

    int electricityReceived = 0;
    int waterReceived = 0;
    int workersGiven = 0;

    bool isActive = false;

    void Start()
    {
        //elecMissingImage.color = Color.blueViolet;
        playerInfo.AddMoney(-houseSO.price);
        playerInfo.AddPopulation(houseSO.population);

        playerInfo.electricityDependants.Add(this);
        playerInfo.workerProviders.Add(this);
        playerInfo.waterDependants.Add(this);

        playerInfo.StartProvidingElectricity();
        playerInfo.StartProvidingWorkers();
        playerInfo.StartProvidingWater();
    }
    public void ProvideWorkers()
    {
        if (!isActive)
            return;

        List<IBuilding> buildingsToRemove = new();

        foreach (var dependant in playerInfo.workerDependants)
        {
            if (workerDependants.ContainsKey(dependant))
                continue;

            if (!HasEnoughWorkers())
            {
                playerInfo.workerProviders.Remove(this);
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
                buildingsToRemove.Add(dependant);
            }
        }

        foreach (IBuilding building in buildingsToRemove)
        {
            playerInfo.workerDependants.Remove(building);
        }
    }
    public void     ReceiveWater(int water)
    {
        waterReceived += water;
        playerInfo.UseWater(water);

        if (HasEnoughWater() && HasEnoughElectricity())
        {
            Activate();
        }
    }
    public void DeleteButton()
    {
        playerInfo.AddMoney(houseSO.price * .8f);
        // ! Might change this in the future to the exact same system as the electricity
        playerInfo.AddPopulation(-houseSO.population);
        playerInfo.UseElectricity(-houseSO.electricityNeeded);
        playerInfo.UseWater(-houseSO.waterNeeded);

        foreach (var (dependant, amountGiven) in workerDependants)
        {
            dependant.Deactivate(0, amountGiven);
            if (!playerInfo.workerDependants.Contains(dependant))
                playerInfo.workerDependants.Add(dependant);
        }

        Destroy(gameObject);
    }

    public void Deactivate(int electricityLost, int workersLost)
    {
        electricityReceived -= electricityLost;
        playerInfo.AddWorkingPopulation(-houseSO.population);

        foreach (var (dependant, amountReceived) in workerDependants)
        {
            dependant.Deactivate(0, amountReceived);
            if (!playerInfo.workerDependants.Contains(dependant))
                playerInfo.workerDependants.Add(dependant);
        }
    }
    public void Activate()
    {
        isActive = true;
        ProvideWorkers();
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        playerInfo.UseElectricity(electricity);
    }

    public IBuildingSO GetBuildingSO() => houseSO;
    public string GetBuildingType() => houseSO.type;
    public string GetRessource() => houseSO.ressource;
    public int GetPopulation() => houseSO.population;
    public int GetPrice() => houseSO.price;
    public int GetElectricityReceived() => electricityReceived;
    public int GetElectricityNeeded() => houseSO.electricityNeeded - electricityReceived;
    public bool CanPlace() => playerInfo.GetMoney >= houseSO.price;
    public bool IsActive() => isActive;
    public bool HasEnoughElectricity() => electricityReceived >= houseSO.electricityNeeded;
    public bool HasEnoughWorkers() => workersGiven < houseSO.population;
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;
    public void SetActive() {
        isActive = true;
        playerInfo.AddPopulation(houseSO.population);
    }
    public void Deactivate()
    {
        isActive = false;
        playerInfo.AddPopulation(-houseSO.population);
    }

    public List<UiDataModel> GetUiDataList()
    {
        return new List<UiDataModel>
        { 
           new UiDataModel(UiDataType.NAME, "House"),
           new UiDataModel(UiDataType.PRICE, houseSO.price.ToString())
        };
    }
    public void DestroyScript() { Destroy(this); }
    public int GetWaterNeeded() => houseSO.waterNeeded - waterReceived;
    public bool HasEnoughWater() => waterReceived >= houseSO.waterNeeded;
}