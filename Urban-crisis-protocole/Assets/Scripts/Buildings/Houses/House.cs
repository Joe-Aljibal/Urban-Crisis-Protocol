using UnityEngine;

public class House : MonoBehaviour, IBuilding
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] public HousesSO houseSO;
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

    public IBuildingSO GetBuildingSO() => houseSO;
    public string GetType() => houseSO.type;
    public string GetRessource() => houseSO.ressource;
    public int GetPopulation() => houseSO.population;
    public int GetPrice() => houseSO.price;
    public int GetElectricityNeeded() => houseSO.electricityNeeded;
    public bool CanPlace() =>
        playerInfo.getMoney >= houseSO.price &&
        playerInfo.getAvailableElectricity >= houseSO.electricityNeeded &&
        playerInfo.getAvailableWater >= houseSO.waterNeeded;

    public bool IsActive() => isActive;
    public void SetActive() {
        isActive = true;
        playerInfo.addPopulation(houseSO.population);
    }
    public void Deactivate()
    {
        isActive = false;
        playerInfo.addPopulation(-houseSO.population);
    }
}