using UnityEngine;
using System.Collections.Generic;

public class Business: MonoBehaviour, IBuilding, IBuildingUiData
{
    [SerializeField] BusinessesSO businessSO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    int electricityReceived = 0;
    public int workersReceived = 0;

    float cooldown = 0;
    bool isActive = false;
    bool isFirstActivation = true;

    void Start()
    {
        playerInfo.AddMoney(-businessSO.GetPrice());

        playerInfo.electricityDependants.Add(this);
        playerInfo.workerDependants.Add(this);
        playerInfo.StartProvidingElectricity();
        playerInfo.StartProvidingWorkers();

        playerInfo.AddResearchPoints(businessSO.GetResearchOutput());
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
            playerInfo.AddMoney(businessSO.GetProfit());
            cooldown = 0;
            return;
        }
        cooldown += Time.deltaTime;
    }

    public void DeleteButton()
    {
        playerInfo.AddMoney(businessSO.GetPrice() * .8f);

        playerInfo.UseElectricity(-businessSO.GetElectricityNeeded());
        Destroy(gameObject);
    }

    public void Deactivate(int electricityLost, int workersLost) {
        isActive = false;
        electricityReceived -= electricityLost;
        workersReceived -= workersLost;

        if (electricityLost > 0)
            playerInfo.electricityDependants.Add(this);

        if (workersLost > 0)
            playerInfo.workerDependants.Add(this);
    }
    public void Activate() {

        if (HasEnoughElectricity() && HasEnoughWorkers())
        {
            isActive = true;
        }

        if (isFirstActivation)
        {
            playerInfo.AddResearchPoints(businessSO.GetResearchOutput());
            isFirstActivation = false;
        }
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        playerInfo.UseElectricity(electricity);

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
    public bool CanPlace() => playerInfo.GetMoney >= businessSO.GetPrice();

    public void ReceiveWorkers(int workers)
    {
        workersReceived += workers;
        playerInfo.AddWorkingPopulation(workers);

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
