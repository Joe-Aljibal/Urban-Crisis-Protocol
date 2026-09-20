using UnityEngine;

public class House : MonoBehaviour, IBuilding
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] public HousesSO houseSO;

    int electricityReceived = 0;

    bool isActive = true;

    void Start()
    {
        playerInfo.addPopulation(houseSO.population);
        playerInfo.UseElectricity(houseSO.electricityNeeded);
        playerInfo.useWater(houseSO.waterNeeded);
    }

    public void DeleteButton()
    {
        playerInfo.addMoney(houseSO.price * .8f);
        playerInfo.addPopulation(-houseSO.population);
        playerInfo.UseElectricity(-houseSO.electricityNeeded);
        playerInfo.useWater(-houseSO.waterNeeded);

        Destroy(gameObject);
    }
    public void Deactivate(int electricityLost)
    {
        isActive = false;
        electricityReceived -= electricityLost;
        playerInfo.electricityList.Add(gameObject);
        playerInfo.addPopulation(-houseSO.population);
    }
    public void Activate()
    {
        isActive = true;
        playerInfo.addPopulation(houseSO.population);
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        playerInfo.UseElectricity(electricity);
        
        if (electricityReceived == GetElectricityNeeded())
            Activate();
    }

    public IBuildingSO GetBuildingSO() => houseSO;
    public string GetType() => houseSO.type;
    public string GetRessource() => houseSO.ressource;
    public int GetPopulation() => houseSO.population;
    public int GetPrice() => houseSO.price;
    public int GetElectricityReceived() => houseSO.electricityReceived;
    public int GetElectricityNeeded() => houseSO.electricityNeeded;
    public bool CanPlace() =>
        playerInfo.getMoney >= houseSO.price &&
        playerInfo.getAvailableElectricity >= houseSO.electricityNeeded &&
        playerInfo.getAvailableWater >= houseSO.waterNeeded;

    public bool IsActive() => isActive;
}