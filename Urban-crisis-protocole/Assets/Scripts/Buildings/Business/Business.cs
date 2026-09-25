using UnityEngine;

public class Business: MonoBehaviour, IBuilding
{
    [SerializeField] BusinessesSO businessesSO;
    PlayerInfo playerinfo = PlayerInfo.Instance;

    int electricityReceived = 0;
    float cooldown = 0;
    bool isActive = true;

    void Start()
    {
        playerinfo.electricityDependants.Add(this, 0);
        playerinfo.ElectrifyBuildings();
        playerinfo.addWorkingPopulation(businessesSO.population);
    }

    void Update()
    {
        if (cooldown >= 1 && isActive)
        {
            playerinfo.addMoney(businessesSO.profit);
            cooldown = 0;
        }
            cooldown = isActive ? cooldown + Time.deltaTime : 0;
    }

    public void DeleteButton()
    {
        playerinfo.addMoney(businessesSO.price * .8f);
        playerinfo.UseElectricity(-businessesSO.electricityNeeded);
        playerinfo.addWorkingPopulation(-businessesSO.population);

        Destroy(gameObject);
    }

    public void Deactivate(int electricityLost) {
        isActive = false;
        electricityReceived -= electricityLost;
        playerinfo.electricityDependants.Add(this, electricityReceived);
        playerinfo.UseElectricity(-electricityLost);
    }
    public void Activate() {
        isActive = true;
        playerinfo.UseElectricity(businessesSO.electricityNeeded);
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
    public int GetPopulation() => businessesSO.population;
    public int GetPrice() => businessesSO.price;
    public bool HasEnoughElectricity() => businessesSO.electricityNeeded <= electricityReceived;
    public int GetElectricityReceived() => electricityReceived;
    public int GetElectricityNeeded() => businessesSO.electricityNeeded;
    public int GetProfit() => businessesSO.profit;
    public bool CanPlace() =>
        playerinfo.getMoney >= businessesSO.price &&
        playerinfo.getAvailableElectricity >= businessesSO.electricityNeeded &&
        playerinfo.getAvailablePopulation >= businessesSO.population;
}
