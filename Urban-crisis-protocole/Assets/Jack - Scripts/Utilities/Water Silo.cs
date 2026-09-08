using UnityEngine;

public class WaterSilo : MonoBehaviour, IBuilding
{
    [SerializeField] WaterSiloSO waterSiloSO;
    PlayerInfo playerInfo = PlayerInfo.Instance;
    
    void Start()
    {
        playerInfo.addWater(waterSiloSO.water);
    }

    public void DeleteButton()
    {
        playerInfo.addMoney(waterSiloSO.price * .8f);
        playerInfo.addWater(-waterSiloSO.water);

        Destroy(gameObject);
    }

    public IBuildingSO getBuildingSO() => waterSiloSO;
    public bool canPlace() => waterSiloSO.price <= playerInfo.getMoney;
    public int getElectricityNeeded() => waterSiloSO.electricityNeeded;
    public int getPrice() => waterSiloSO.price;
    public string getRessource() => waterSiloSO.ressource;
    public string getType() => waterSiloSO.type;
    public int getWater => waterSiloSO.water;
}
