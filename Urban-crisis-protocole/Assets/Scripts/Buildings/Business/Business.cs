using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Business: MonoBehaviour, IBuilding, IBuildingUiData
{
    [SerializeField] BusinessesSO businessSO;
    //[SerializeField] Image workersImage;
    //[SerializeField] Image electricityImage;
    PlayerInfo playerinfo = PlayerInfo.Instance;

    int electricityReceived = 0;
    public int workersReceived = 0;

    float cooldown = 0;
    bool isActive = false;

    void Start()
    {
        playerinfo.AddMoney(-businessSO.price);

        playerinfo.electricityDependants.Add(this);
        playerinfo.workerDependants.Add(this);
        playerinfo.StartProvidingElectricity();
        playerinfo.StartProvidingWorkers();
    }

    void Update()
    {
        //electricityImage.color = HasEnoughElectricity() ? Color.green : Color.red;
        //workersImage.color = HasEnoughWorkers() ? Color.green : Color.red;

        if (!isActive)
        {
            cooldown = 0;
            return;
        }

        if (cooldown >= 1)
        {
            playerinfo.AddMoney(businessSO.profit);
            cooldown = 0;
            return;
        }
        cooldown += Time.deltaTime;
    }

    public void DeleteButton()
    {
        playerinfo.AddMoney(businessSO.price * .8f);

        playerinfo.UseElectricity(-businessSO.electricityNeeded);
        Destroy(gameObject);
    }

    public void Deactivate(int electricityLost, int workersLost) {
        isActive = false;
        electricityReceived -= electricityLost;
        workersReceived -= workersLost;

        if (electricityLost > 0)
            playerinfo.electricityDependants.Add(this);

        if (workersLost > 0)
            playerinfo.workerDependants.Add(this);
    }
    public void Activate() {

        if (HasEnoughElectricity() && HasEnoughWorkers())
        {
            isActive = true;
        }
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        playerinfo.UseElectricity(electricity);

        if (electricityReceived >= GetElectricityNeeded() && workersReceived >= GetWorkersNeeded())
        {
            Activate();
        }
    }

    public bool IsActive() => isActive;
    public IBuildingSO GetBuildingSO() => businessSO;
    public string GetBuildingType() => businessSO.type;
    public string GetRessource() => businessSO.ressource;
    public int GetWorkerPopulation() => workersReceived;
    public int GetPrice() => businessSO.price;
    public bool HasEnoughElectricity() => businessSO.electricityNeeded <= electricityReceived;
    public bool HasEnoughWorkers() => businessSO.workersNeeded <= workersReceived;
    public int GetElectricityReceived() => electricityReceived;
    public int GetProfit() => businessSO.profit;
    public bool CanPlace() => playerinfo.GetMoney >= businessSO.price;

    public void ReceiveWorkers(int workers)
    {
        workersReceived += workers;
        playerinfo.AddWorkingPopulation(workers);

        if (workersReceived >= GetWorkersNeeded() && electricityReceived >= GetElectricityNeeded())
        {
            Activate();
        }
    }

    public int GetWorkersNeeded() => businessSO.workersNeeded - workersReceived;
    public int GetPopulation() => businessSO.workersNeeded;
    public int GetElectricityNeeded() => businessSO.electricityNeeded;

    public List<UiDataModel> GetUiDataList()
    {

        
        return new List<UiDataModel>
        { 
           new UiDataModel(UiDataType.NAME, businessSO.type),
           new UiDataModel(UiDataType.PRICE, businessSO.price.ToString())
        };
    }

    public void DestroyScript() { Destroy(this); }
}
