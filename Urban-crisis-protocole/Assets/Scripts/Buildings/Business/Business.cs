using UnityEngine;
using UnityEngine.UI;

public class Business: MonoBehaviour, IBuilding
{
    [SerializeField] BusinessesSO businessesSO;
    [SerializeField] Image workersImage;
    [SerializeField] Image electricityImage;
    PlayerInfo playerinfo = PlayerInfo.Instance;

    int electricityReceived = 0;
    public int workersReceived = 0;

    float cooldown = 0;
    bool isActive = false;

    void Start()
    {
        playerinfo.electricityDependants.Add(this);
        playerinfo.workerDependants.Add(this);
        playerinfo.ElectrifyBuildings();
        playerinfo.EmployWorkers();
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
            playerinfo.addMoney(businessesSO.profit);
            cooldown = 0;
        }
        cooldown = cooldown + Time.deltaTime;
    }

    public void DeleteButton()
    {
        playerinfo.addMoney(businessesSO.price * .8f);

        playerinfo.UseElectricity(-businessesSO.electricityNeeded);
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

        if (electricityReceived == GetElectricityNeeded())
        {
            Activate();
            playerinfo.electricityDependants.Remove(this);
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
    public int GetElectricityNeeded() => businessesSO.electricityNeeded - electricityReceived;
    public int GetProfit() => businessesSO.profit;
    public bool CanPlace() => playerinfo.getMoney >= businessesSO.price;

    public void ReceiveWorkers(int workersReceived)
    {
        this.workersReceived += workersReceived;
        playerinfo.addWorkingPopulation(workersReceived);
    }

    public int GetWorkersNeeded() => businessesSO.workersNeeded - workersReceived;
}
