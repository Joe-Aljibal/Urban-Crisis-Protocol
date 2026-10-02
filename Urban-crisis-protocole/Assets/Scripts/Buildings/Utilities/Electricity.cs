using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Electricity : MonoBehaviour, IBuilding, IBuildingUiData
{
    [SerializeField] public ElectricitySO electricitySO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    bool isActive = true;
    Queue<GameObject> electricityDependants = new Queue<GameObject>();

    private void Start()
    {
        playerInfo.ElectrifyBuildings(electricitySO);
        playerInfo.addElectricity(electricitySO.electricityOutput);
    }

    public void DeleteButton()
    {
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        foreach (GameObject go in electricityDependants)
        {
            go.GetComponent<IBuilding>().Deactivate();
            playerInfo.electricityList.Add(go);
        }

        Destroy(gameObject);
    }

    public IBuildingSO GetBuildingSO() => electricitySO;
    public string GetType() => electricitySO.type;
    public string GetRessource() => electricitySO.ressource;
    public int GetElectricityNeeded() => 0;
    public int GetPrice() => electricitySO.price;
    public bool CanPlace() => playerInfo.getMoney >= electricitySO.price;
    public bool IsActive() => isActive;
    public void SetActive() {}
    public void Deactivate() {
        isActive = false;
        playerInfo.addElectricity(-electricitySO.electricityOutput);
        foreach (GameObject go in electricityDependants)
        {
            go.GetComponent<IBuilding>().Deactivate();
            playerInfo.electricityList.Add(go);
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
