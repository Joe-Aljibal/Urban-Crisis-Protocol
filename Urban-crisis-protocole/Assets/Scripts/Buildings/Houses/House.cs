using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour, IBuilding, IWorkerProvider
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
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
        playerInfo.addMoney(-houseSO.price);
        playerInfo.addPopulation(houseSO.population);
        playerInfo.useWater(houseSO.waterNeeded);

        playerInfo.electricityDependants.Add(this);
        playerInfo.workerProviders.Add(this);
        playerInfo.ElectrifyBuildings();
        playerInfo.EmployWorkers();


    }
    public void EmployWorkers()
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
                dependant.Activate();
                buildingsToRemove.Add(dependant);
            }
        }

        foreach (IBuilding building in buildingsToRemove)
        {
            playerInfo.workerDependants.Remove(building);
        }
    }
    public void DeleteButton()
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        playerInfo.addMoney(houseSO.price * .8f);
        // ! Might change this in the future to the exact same system as the electricity
        playerInfo.addPopulation(-houseSO.population);
        playerInfo.UseElectricity(-houseSO.electricityNeeded);
        playerInfo.useWater(-houseSO.waterNeeded);

        foreach (var (dependant, amountGiven) in workerDependants)
        {
            dependant.Deactivate(0, amountGiven);
            if (!playerInfo.workerDependants.Contains(dependant))
                playerInfo.workerDependants.Add(dependant);
        }
    }
    public void Deactivate(int electricityLost, int workersLost)
    {
        electricityReceived -= electricityLost;
        playerInfo.addWorkingPopulation(-houseSO.population);

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
        playerInfo.addWorkingPopulation(houseSO.population);
        EmployWorkers();
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
    public bool CanPlace() => playerInfo.getMoney >= houseSO.price;
    public bool IsActive() => isActive;
    public bool HasEnoughElectricity() => electricityReceived >= houseSO.electricityNeeded;
    public bool HasEnoughWorkers() => workersGiven < houseSO.population;
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;
}