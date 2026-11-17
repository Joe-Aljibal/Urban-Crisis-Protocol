using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GambleBuilding : MonoBehaviour, IBuilding, ISellable, IBuildingUiData
{
    [SerializeField] GambleBuildingSO gambleBuildingSO;
    //[SerializeField] Image workersImage;
    //[SerializeField] Image electricityImage;
    CityData playerinfo = Cities.cities["PLAYER"];

    int electricityReceived = 0;
    public int workersReceived = 0;

    float cooldown = 0;
    bool isActive = false;

    void Start()
    {


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
            //playerinfo.AddMoney(gambleBuildingSO.profit);
            cooldown = 0;
            return;
        }
        cooldown += Time.deltaTime;
    }

    public void DeleteButton()
    {
        playerinfo.AddMoney(gambleBuildingSO.price * .8f);

        playerinfo.UseElectricity(-gambleBuildingSO.electricityNeeded);
        Destroy(gameObject);
    }

    public void Deactivate(int electricityLost, int workersLost)
    {
        isActive = false;
        electricityReceived -= electricityLost;
        workersReceived -= workersLost;

        if (electricityLost > 0)
            playerinfo.electricityDependants.Add(this);

        if (workersLost > 0)
            playerinfo.workerDependants.Add(this);
    }
    public void Activate()
    {

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
    public IBuildingSO GetBuildingSO() => gambleBuildingSO;
    public string GetBuildingType() => gambleBuildingSO.type;
    public string GetRessource() => gambleBuildingSO.ressource;
    public int GetWorkerPopulation() => workersReceived;
    public int GetPrice() => gambleBuildingSO.price;
    public bool HasEnoughElectricity() => gambleBuildingSO.electricityNeeded <= electricityReceived;
    public bool HasEnoughWorkers() => gambleBuildingSO.workersNeeded <= workersReceived;
    public int GetElectricityReceived() => electricityReceived;
    public int GetProfit() => gambleBuildingSO.profit;
    public bool CanPlace() => playerinfo.GetMoney >= gambleBuildingSO.price;

    public void ReceiveWorkers(int workers)
    {
        workersReceived += workers;
        playerinfo.AddWorkingPopulation(workers);

        if (workersReceived >= GetWorkersNeeded() && electricityReceived >= GetElectricityNeeded())
        {
            Activate();
        }
    }

    public void ReceiveElectricity(int electricity)
    {
        electricityReceived += electricity;

        if (electricityReceived >= GetElectricityNeeded() && workersReceived >= GetWorkersNeeded())
        {
            Activate();
        }
    }

    public int GetWorkersNeeded() => gambleBuildingSO.workersNeeded - workersReceived;
    public int GetPopulation() => gambleBuildingSO.workersNeeded;
    public int GetElectricityNeeded() => gambleBuildingSO.electricityNeeded;

    public List<UiDataModel> GetUiDataList()
    {


        return new List<UiDataModel>
        {
           new UiDataModel(UiDataType.NAME, gambleBuildingSO.type),
           new UiDataModel(UiDataType.PRICE, gambleBuildingSO.price.ToString())
        };
    }

    public void DestroyScript() { Destroy(this); }
    public void SetCityData(CityData data) => playerinfo = data ?? Cities.cities["PLAYER"];

    public void Sell() { playerinfo.RemoveBuilding(gameObject); DeleteButton(); }

}
