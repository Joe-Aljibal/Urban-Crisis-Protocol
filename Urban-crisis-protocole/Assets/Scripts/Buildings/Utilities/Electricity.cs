using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Electricity : MonoBehaviour, IBuilding
{
    [SerializeField] public ElectricitySO electricitySO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    bool isActive = true;
    Dictionary<GameObject, int> electricityDependants = new Dictionary<GameObject, int>();

    private void Start()
    {
        playerInfo.ElectrifyBuildings(electricitySO);
        playerInfo.addElectricity(electricitySO.electricityOutput);

        ElectrifyBuildings();
    }

    // ! Next thing to do is to create Action/Events so that whenever a new buildingg that requires electricity is added we go through the list of electric providers that have room left.
    // ! Create that list of providers in playerInfo.
    public void ElectrifyBuildings()
    {
        foreach (var go in playerInfo.electricityList)
        {
            bool electricityLeft = electricitySO.electricityOutput - electricitySO.electricityGiven > 0;

            if (electricityLeft)
            {
                int electricityAvailable = electricitySO.electricityOutput - electricitySO.electricityGiven;
                int electricityNeeded = go.Key.GetElectricityNeeded() - go.Key.GetElectricityReceived();
                go.Key.UseElectricity(Mathf.Min(electricityAvailable, electricityNeeded));
            }
            else
            {
                break;
            }
        }
    }

    public void DeleteButton()
    {
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        foreach (var pair in electricityDependants)
        {
            pair.Key.GetComponent<IBuilding>().Deactivate(pair.Value);
            playerInfo.electricityList.Add(pair.Key.GetComponent<IBuilding>(), pair.Value);
        }

        Destroy(gameObject);
    }
    public void Deactivate(int x)
    {
        isActive = false;
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        foreach (var pair in electricityDependants)
        {
            pair.Key.GetComponent<IBuilding>().Deactivate(pair.Value);
            playerInfo.electricityList.Add(pair.Key.GetComponent<IBuilding>(), pair.Value);
        }
    }

    public IBuildingSO GetBuildingSO() => electricitySO;
    public string GetType() => electricitySO.type;
    public string GetRessource() => electricitySO.ressource;
    public int GetElectricityNeeded() => 0;
    public int GetPrice() => electricitySO.price;
    public bool CanPlace() => playerInfo.getMoney >= electricitySO.price;
    public bool IsActive() => isActive;
    public void SetActive() {}
    public void Activate() { }
    public void UseElectricity(int electricity) { }
}
