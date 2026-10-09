using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IBuilding , IBuildingUiData
{
    [SerializeField] WaterSO waterSO;
    PlayerInfo playerInfo = PlayerInfo.Instance;
    bool isActive = true;
    
    void Start()
    {
        playerInfo.addWater(waterSO.water);
    }

    public void DeleteButton()
    {
        playerInfo.addMoney(waterSO.price * .8f);
        playerInfo.addWater(-waterSO.water);

        Destroy(gameObject);
    }

    public IBuildingSO GetBuildingSO() => waterSO;
    public bool CanPlace() => waterSO.price <= playerInfo.getMoney;
    public int GetElectricityNeeded() => waterSO.electricityNeeded;
    public int GetPrice() => waterSO.price;
    public string GetRessource() => waterSO.ressource;
    public bool HasEnoughElectricity() => true;
    public string GetBuildingType() => waterSO.type;
    public int GetWater => waterSO.water;
    public bool IsActive() => isActive;

    public void SetActive()
    {
        isActive = true;
        playerInfo.addWater(waterSO.water);
    }

    public void Deactivate() {
        isActive = false;
        playerInfo.addWater(-waterSO.water);
    }

    public int GetElectricityReceived() => 0;

    public void Deactivate(int electricityLost, int workersLost)
    {
        throw new System.NotImplementedException();
    }

    public List<UiDataText> GetUiTextList()
    {
        
        return new List<UiDataText>
        { 
           
        };
    }

    public List<UiDataInteraction> GetUiInteractionList()
    {
        return new List<UiDataInteraction>
        {
            
        };
    }
    public void Activate() { }
    public void UseElectricity(int electricity) { }
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;

    public bool HasEnoughWorkers() => true;

   
   
}
