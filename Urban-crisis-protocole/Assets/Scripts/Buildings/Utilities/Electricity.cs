using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Electricity : MonoBehaviour, IBuilding
{
    [SerializeField] public ElectricitySO electricitySO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    int electricityGiven = 0;
    bool isActive = true;
    Dictionary<IBuilding, int> electricityDependants = new();

    private void Start()
    {
        playerInfo.addElectricity(electricitySO.electricityOutput);
        playerInfo.electricityProviders.Add(gameObject, electricitySO.electricityOutput);
        ElectrifyBuildings();
    }

    public void ElectrifyBuildings()
    {
        foreach (var pair in playerInfo.electricityDependants)
        {
            bool electricityLeft = electricitySO.electricityOutput - electricityGiven > 0;

            if (!electricityLeft)
            {
                playerInfo.electricityProviders.Remove(gameObject);
                break;
            }

            int electricityAvailable = electricitySO.electricityOutput - electricityGiven;
            int electricityNeeded = pair.Key.GetElectricityNeeded() - pair.Key.GetElectricityReceived();
            int amountToGive = Mathf.Min(electricityAvailable, electricityNeeded);
            pair.Key.UseElectricity(amountToGive);
            electricityGiven += amountToGive;
            playerInfo.UseElectricity(amountToGive);
            electricityDependants.Add(pair.Key, amountToGive);
        }
    }

    public void DeleteButton()
    {
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        foreach (var pair in electricityDependants)
        {
            pair.Key.Deactivate(pair.Value);
            playerInfo.electricityDependants.Add(pair.Key, pair.Value);
        }

        Destroy(gameObject);
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
    public int GetElectricityReceived() => 0;
    public int GetElectricityNeeded() => 0;
    public int GetPrice() => electricitySO.price;
    public bool CanPlace() => playerInfo.getMoney >= electricitySO.price;
    public bool IsActive() => isActive;
    public void UseElectricity(int x) { }
}
