using UnityEngine;

public class House : MonoBehaviour, IBuilding
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] public HousesSO houseSO;
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

    public IBuildingSO getBuildingSO() => houseSO;
    public string getType() => houseSO.type;
    public string getRessource() => houseSO.ressource;
    public int getPopulation() => houseSO.population;
    public int getPrice() => houseSO.price;
    public int getElectricityNeeded() => houseSO.electricityNeeded;
    public bool canPlace() =>
        playerInfo.getMoney >= houseSO.price &&
        playerInfo.getAvailableElectricity >= houseSO.electricityNeeded &&
        playerInfo.getAvailableWater >= houseSO.waterNeeded;
}