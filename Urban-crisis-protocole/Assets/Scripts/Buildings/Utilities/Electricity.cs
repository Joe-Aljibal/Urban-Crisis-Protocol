using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Electricity : MonoBehaviour, IBuilding, IElectricityProvider
{
    [SerializeField] public ElectricitySO electricitySO;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    int electricityGiven = 0;
    bool isActive = true;
    // The building and how much it received from this script
    Dictionary<IBuilding, int> electricityDependants = new();

    private void Start()
    {
        playerInfo.addElectricity(electricitySO.electricityOutput);
        playerInfo.electricityProviders.Add(this);
        ElectrifyBuildings();
    }

    public void ElectrifyBuildings()
    {
        List<IBuilding> buildingsToRemove = new();

        foreach (var dependant in playerInfo.electricityDependants)
        {
            if (electricityDependants.ContainsKey(dependant))
                continue;

            if (!CanGiveElectricity())
            {
                playerInfo.electricityProviders.Remove(this);
                Debug.Log("No more electricity left");
                break;
            }

            int electricityAvailable = electricitySO.electricityOutput - electricityGiven;
            int electricityNeeded = dependant.GetElectricityNeeded();
            int amountToGive = Mathf.Min(electricityAvailable, electricityNeeded);

            dependant.UseElectricity(amountToGive);
            electricityGiven += amountToGive;
            electricityDependants.Add(dependant, amountToGive);

            if (dependant.HasEnoughElectricity())
            {
                dependant.Activate();
                buildingsToRemove.Add(dependant);
            }
        }

        foreach (IBuilding building in buildingsToRemove)
        {
            playerInfo.electricityDependants.Remove(building);
        }
    }
    public void DeleteButton()
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        playerInfo.electricityProviders.Remove(this);
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        foreach (var (dependant, amountReceived) in electricityDependants)
        {
            dependant.Deactivate(amountReceived, 0);

            if (!playerInfo.electricityDependants.Contains(dependant))
                playerInfo.electricityDependants.Add(dependant);
        }
        playerInfo.ElectrifyBuildings();
    }

    public void Deactivate(int x, int y)
    {
        isActive = false;
        playerInfo.addElectricity(-electricitySO.electricityOutput);
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
        playerInfo.addElectricity(electricitySO.electricityOutput);
        ElectrifyBuildings();
    }
    public string GetBuildingType() => electricitySO.type;
    public string GetRessource() => electricitySO.ressource;
    // Tracks if it can still give out elecrticity
    public bool HasEnoughElectricity() => true;
    public bool CanGiveElectricity() => electricityGiven < electricitySO.electricityOutput;
    public int GetElectricityReceived() => 0;
    public int GetElectricityNeeded() => 0;
    public int GetPrice() => electricitySO.price;
    public bool CanPlace() => playerInfo.getMoney >= electricitySO.price;
    public bool IsActive() => isActive;
    public void UseElectricity(int x) { }

    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;
}
