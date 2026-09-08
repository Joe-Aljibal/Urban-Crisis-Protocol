using UnityEngine;

public class Business: MonoBehaviour, IBuilding
{
    PlayerInfo playerinfo = PlayerInfo.Instance;
    float cooldown = 0;
    [SerializeField] BusinessesSO businessesSO;
    void Start()
    {
        playerinfo.addWorkingPopulation(businessesSO.population);
        playerinfo.UseElectricity(businessesSO.electricityNeeded);
    }

    void Update()
    {
        if (cooldown >= 1)
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

    public IBuildingSO getBuildingSO() => businessesSO;
    public string getType() => businessesSO.type;
    public string getRessource() => businessesSO.ressource;
    public int getPopulation() => businessesSO.population;
    public int getPrice() => businessesSO.price;
    public int getElectricityNeeded() => businessesSO.electricityNeeded;
    public int getProfit() => businessesSO.profit;
    public bool canPlace() =>
        playerinfo.getMoney >= businessesSO.price &&
        playerinfo.getAvailableElectricity >= businessesSO.electricityNeeded &&
        playerinfo.getAvailablePopulation >= businessesSO.population;
}
