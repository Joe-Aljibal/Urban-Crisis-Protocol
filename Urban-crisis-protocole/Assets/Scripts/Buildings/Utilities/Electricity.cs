using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Electricity : MonoBehaviour, IBuilding
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
        playerInfo.electricityProviders.Add(this, electricitySO.electricityOutput);
        ElectrifyBuildings();
    }

    public void ElectrifyBuildings()
    {
        List<IBuilding> buildingsToRemove = new();
        foreach (var pair in playerInfo.electricityDependants)
        {
            if (electricityDependants.ContainsKey(pair.Key))
                continue;

            bool electricityLeft = electricitySO.electricityOutput - electricityGiven > 0;

            if (!electricityLeft)
            {
                playerInfo.electricityProviders.Remove(this);
                Debug.Log("No more electricity left");
                break;
            }

            int electricityAvailable = electricitySO.electricityOutput - electricityGiven;
            int electricityNeeded = pair.Key.GetElectricityNeeded() - pair.Key.GetElectricityReceived();
            int amountToGive = Mathf.Min(electricityAvailable, electricityNeeded);

            pair.Key.UseElectricity(amountToGive);
            electricityGiven += amountToGive;
            electricityDependants.Add(pair.Key, amountToGive);

            if (pair.Key.HasEnoughElectricity())
            {
                pair.Key.Activate();
                buildingsToRemove.Add(pair.Key);
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
            dependant.Deactivate(amountReceived);

            if (!playerInfo.electricityDependants.ContainsKey(dependant))
                playerInfo.electricityDependants.Add(dependant, dependant.GetElectricityNeeded() - dependant.GetElectricityReceived());
        }
        playerInfo.ElectrifyBuildings();
    }

    public void Deactivate(int x)
    {
        isActive = false;
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        electricityGiven = 0;
        foreach (var pair in electricityDependants)
        {
            pair.Key.Deactivate(pair.Value);
            if (!playerInfo.electricityDependants.ContainsKey(pair.Key))
                playerInfo.electricityDependants.Add(pair.Key, pair.Value);
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
    public bool HasEnoughElectricity() => electricityGiven < electricitySO.electricityOutput;
    public int GetElectricityReceived() => 0;
    public int GetElectricityNeeded() => 0;
    public int GetPrice() => electricitySO.price;
    public bool CanPlace() => playerInfo.getMoney >= electricitySO.price;
    public bool IsActive() => isActive;
    public void UseElectricity(int x) { }
}
