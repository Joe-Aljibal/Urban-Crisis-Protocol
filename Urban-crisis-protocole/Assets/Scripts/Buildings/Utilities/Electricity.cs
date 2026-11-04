using System.Collections.Generic;
using UnityEngine;

public class Electricity : MonoBehaviour, IBuilding, IElectricityProvider, IBuildingUiData
{
    [SerializeField] public ElectricitySO electricitySO;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    int electricityGiven = 0;
    bool isActive = true;
    // The building and how much it received from this script
    Dictionary<IBuilding, int> electricityDependants = new();

    private void Start()
    {
        playerInfo.AddElectricity(electricitySO.electricityOutput);
        playerInfo.electricityProviders.Add(this);
        ProvideElectricity();
    }

    public void ProvideElectricity()
    {
        List<IBuilding> buildingsToRemove = new();

        foreach (IBuilding dependant in playerInfo.electricityDependants)
        {
            if (electricityDependants.ContainsKey(dependant))
                continue;

            if (!HasAvailableElectricity())
                break;

            int electricityAvailable = electricitySO.electricityOutput - electricityGiven;
            int electricityNeeded = dependant.GetElectricityNeeded();
            int amountToGive = Mathf.Min(electricityAvailable, electricityNeeded);

            dependant.UseElectricity(amountToGive);
            electricityGiven += amountToGive;
            electricityDependants.Add(dependant, amountToGive);

            if (dependant.HasEnoughElectricity())
                buildingsToRemove.Add(dependant);
        }

        foreach (IBuilding building in buildingsToRemove)
        {
            playerInfo.electricityDependants.Remove(building);
        }
    }
    public void DeleteButton()
    {
        playerInfo.electricityProviders.Remove(this);
        playerInfo.AddElectricity(-electricitySO.electricityOutput);
        foreach (var (dependant, amountReceived) in electricityDependants)
        {
            dependant.Deactivate(amountReceived, 0);

            if (!playerInfo.electricityDependants.Contains(dependant))
                playerInfo.electricityDependants.Add(dependant);
        }
        playerInfo.StartProvidingElectricity();
        Destroy(gameObject);
    }

    public void Deactivate(int x, int y)
    {
        isActive = false;
        playerInfo.AddElectricity(-electricitySO.electricityOutput);
        electricityGiven = 0;
        foreach (var (dependant, amountReceived) in electricityDependants)
        {
            dependant.Deactivate(amountReceived, 0);
            if (!playerInfo.electricityDependants.Contains(dependant))
                playerInfo.electricityDependants.Add(dependant);
        }
    }

    public IBuildingSO GetBuildingSO() => electricitySO;
    public void Activate()
    {
        isActive = true;
        playerInfo.AddElectricity(electricitySO.electricityOutput);
        ProvideElectricity();
    }
    public string GetBuildingType() => electricitySO.type;
    public string GetRessource() => electricitySO.ressource;
    // Tracks if it can still give out elecrticity
    public bool HasEnoughElectricity() => true;
    public bool HasAvailableElectricity() => electricityGiven < electricitySO.electricityOutput;
    public int GetElectricityReceived() => 0;
    public int GetElectricityNeeded() => 0;
    public int GetPrice() => electricitySO.price;
    public bool CanPlace() => playerInfo.GetMoney >= electricitySO.price;
    public bool IsActive() => isActive;
    public void UseElectricity(int x) { }

    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;
    public void SetActive() {}
    public void Deactivate() {
        isActive = false;
        playerInfo.AddElectricity(-electricitySO.electricityOutput);
        foreach (var (dependant, amountToGive) in electricityDependants)
        {
            dependant.Deactivate(amountToGive, 0);
            playerInfo.electricityDependants.Add(dependant);
        }
    }
    

    // clean later same thing everywhere use parent of sciprtableobject
     public List<UiDataModel> GetUiDataList()
    {
        
        return new List<UiDataModel>
        { 
           new UiDataModel(UiDataType.NAME, electricitySO.type),
           new UiDataModel(UiDataType.PRICE, electricitySO.price.ToString())
        };
    }
}
