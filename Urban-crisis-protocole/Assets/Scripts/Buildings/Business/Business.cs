using UnityEngine;

public class Business: MonoBehaviour, IBuilding
{
    [SerializeField] BusinessesSO businessesSO;
    
    PlayerInfo playerinfo = PlayerInfo.Instance;
    
    float cooldown = 0;
    bool isActive = true;


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
            cooldown += Time.deltaTime;
    }

    public void DeleteButton()
    {
        playerinfo.addMoney(businessesSO.price * .8f);
        playerinfo.UseElectricity(-businessesSO.electricityNeeded);
        playerinfo.addWorkingPopulation(-businessesSO.population);

        Destroy(gameObject);
    }

    public void Deactivate() {
        isActive = false;
        playerinfo.electricityList.Add(gameObject);
        playerinfo.UseElectricity(-businessesSO.electricityNeeded);
    }
    public bool IsActive() => isActive;
    public void SetActive() {
        isActive = true;
        playerinfo.UseElectricity(businessesSO.electricityNeeded);

    }

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
