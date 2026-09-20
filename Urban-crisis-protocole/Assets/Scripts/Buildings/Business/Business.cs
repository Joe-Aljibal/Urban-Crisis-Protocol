using UnityEngine;

public class Business: MonoBehaviour, IBuilding
{
    [SerializeField] BusinessesSO businessesSO;
    
    PlayerInfo playerinfo = PlayerInfo.Instance;
    
    float cooldown = 0;
    bool isActive = true;

    int electricityReceived = 0;

    void Start()
    {
        playerinfo.addWorkingPopulation(businessesSO.population);
        playerinfo.UseElectricity(businessesSO.electricityNeeded);
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
        playerinfo.electricityList.Add(gameObject);
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
            Activate();
    }

    public bool IsActive() => isActive;
    public IBuildingSO GetBuildingSO() => businessesSO;
    public string GetType() => businessesSO.type;
    public string GetRessource() => businessesSO.ressource;
    public int GetPopulation() => businessesSO.population;
    public int GetPrice() => businessesSO.price;
    public int GetElectricityNeeded() => businessesSO.electricityNeeded;
    public int GetProfit() => businessesSO.profit;
    public bool CanPlace() =>
        playerinfo.getMoney >= businessesSO.price &&
        playerinfo.getAvailableElectricity >= businessesSO.electricityNeeded &&
        playerinfo.getAvailablePopulation >= businessesSO.population;
}
