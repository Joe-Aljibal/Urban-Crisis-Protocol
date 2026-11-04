using UnityEngine;
using System.Collections.Generic;

public class Business: MonoBehaviour, IBuilding, IBuildingUiData
{
    [SerializeField] BusinessesSO businessSO;
    PlayerInfo playerinfo = PlayerInfo.Instance;

    int electricityReceived = 0;
    public int workersReceived = 0;

    float cooldown = 0;
    bool isActive = false;
    bool isFirstActivation = true;

    void Start()
    {
        playerinfo.AddMoney(-businessSO.GetPrice());

        playerinfo.electricityDependants.Add(this);
        playerinfo.workerDependants.Add(this);
        playerinfo.StartProvidingElectricity();
        playerinfo.StartProvidingWorkers();
    }

    void Update()
    {
        if (!isActive)
        {
            cooldown = 0;
            return;
        }

        if (cooldown >= 1)
        {
            playerinfo.AddMoney(businessSO.GetProfit());
            cooldown = 0;
            return;
        }
        cooldown += Time.deltaTime;
    }

    public void DeleteButton()
    {
        playerinfo.AddMoney(businessSO.GetPrice() * .8f);

        playerinfo.UseElectricity(-businessSO.GetElectricityNeeded());
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

        if (isFirstActivation)
        {
            playerinfo.AddResearchPoints(businessSO.GetResearchOutput());
            isFirstActivation = false;
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
    public string GetRessource() => businessSO.GetRessource();
    public int GetWorkerPopulation() => workersReceived;
    public int GetPrice() => businessSO.GetPrice();
    public bool HasEnoughElectricity() => businessSO.GetElectricityNeeded() <= electricityReceived;
    public bool HasEnoughWorkers() => businessSO.GetWorkersNeeded() <= workersReceived;
    public int GetElectricityReceived() => electricityReceived;
    public int GetProfit() => businessSO.GetProfit();
    public bool CanPlace() => playerinfo.GetMoney >= businessSO.GetPrice();

    public void ReceiveWorkers(int workers)
    {
        workersReceived += workers;
        playerinfo.AddWorkingPopulation(workers);

        if (workersReceived >= GetWorkersNeeded() && electricityReceived >= GetElectricityNeeded())
        {
            Activate();
        }
    }

    public int GetWorkersNeeded() => businessSO.GetWorkersNeeded() - workersReceived;
    public int GetPopulation() => businessSO.GetWorkersNeeded();
    public int GetElectricityNeeded() => businessSO.GetElectricityNeeded();

    public List<UiDataModel> GetUiDataList()
    {

        
        return new List<UiDataModel>
        { 
           new UiDataModel(UiDataType.NAME, "businessSO.type"),
           new UiDataModel(UiDataType.PRICE, businessSO.GetPrice().ToString())
        };
    }

}
