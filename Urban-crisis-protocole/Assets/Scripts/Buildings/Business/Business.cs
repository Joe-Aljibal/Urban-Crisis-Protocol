using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Business: MonoBehaviour, IBuilding, IBuildingUiData, ISellable
{
    [SerializeField] BusinessesSO businessesSO;
    [SerializeField] Image workersImage;
    [SerializeField] Image electricityImage;
    
    
    CityData cityData;
    int electricityReceived = 0;
    public int workersReceived = 0;

    float cooldown = 0;
    bool isActive = false;

    void Start()
    {
        cityData.electricityDependants.Add(this);
        cityData.workerDependants.Add(this);
        cityData.ElectrifyBuildings();
        cityData.EmployWorkers();
    }

    void Update()
    {
        electricityImage.color = HasEnoughElectricity() ? Color.green : Color.red;
        workersImage.color = HasEnoughWorkers() ? Color.green : Color.red;
        
        if (!isActive)
        {
            cooldown = 0;
            return;
        }

        if (cooldown >= 1)
        {
            cityData.addMoney(businessesSO.profit);
            cooldown = 0;
        }
        cooldown = cooldown + Time.deltaTime;
    }

    public void Sell()
    {
        cityData.addMoney(businessesSO.price * .8f);

        cityData.UseElectricity(-businessesSO.electricityNeeded);
        Destroy(gameObject);
    }

    public void SetCityData(CityData cityData)
    {
        this.cityData = cityData;
    }

    public void Deactivate(int electricityLost, int workersLost) {
        isActive = false;
        electricityReceived -= electricityLost;
        workersReceived -= workersLost;

        if (electricityLost > 0)
            cityData.electricityDependants.Add(this);

        if (workersLost > 0)
            cityData.workerDependants.Add(this);
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
        cityData.UseElectricity(electricity);

        if (electricityReceived == GetElectricityNeeded())
        {
            Activate();
            cityData.electricityDependants.Remove(this);
        }
    }

    public bool IsActive() => isActive;
    public IBuildingSO GetBuildingSO() => businessesSO;
    public string GetBuildingType() => businessesSO.type;
    public string GetRessource() => businessesSO.ressource;
    public int GetWorkerPopulation() => workersReceived;
    public int GetPrice() => businessesSO.price;
    public bool HasEnoughElectricity() => businessesSO.electricityNeeded <= electricityReceived;
    public bool HasEnoughWorkers() => businessesSO.workersNeeded <= workersReceived;
    public int GetElectricityReceived() => electricityReceived;
    public int GetProfit() => businessesSO.profit;
    public bool CanPlace() => cityData.getMoney >= businessesSO.price;

    public void ReceiveWorkers(int workersReceived)
    {
        this.workersReceived += workersReceived;
        cityData.addWorkingPopulation(workersReceived);
    }

    public string GetType() => businessesSO.type;
    public int GetPopulation() => businessesSO.workersNeeded;
    public int GetElectricityNeeded() => businessesSO.electricityNeeded;

    public List<UiDataText> GetUiTextList()
    {

        
        return new List<UiDataText>
        { 
           new UiDataText(UiDataType.NAME, businessesSO.type),
           new UiDataText(UiDataType.PRICE, "Current price : "  + businessesSO.price.ToString())

        };
    }

    public List<UiDataInteraction> GetUiInteractionList()
    {
        return new List<UiDataInteraction>
        {
            new UiDataInteraction(UiInteractionType.SELL, new Sell())
        };
    }
    public int GetWorkersNeeded() => businessesSO.workersNeeded - workersReceived;
}
