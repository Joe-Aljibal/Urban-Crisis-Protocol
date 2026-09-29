using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour, IBuilding
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] public HousesSO houseSO;
    [SerializeField] Image elecMissingImage;
    [SerializeField] TMP_Text label;

    int electricityReceived = 0;

    bool isActive = false;

    void Start()
    {
        elecMissingImage.color = Color.blueViolet;
        playerInfo.addPopulation(houseSO.population);
        playerInfo.useWater(houseSO.waterNeeded);

        playerInfo.electricityDependants.Add(this, houseSO.electricityNeeded);
        playerInfo.ElectrifyBuildings();
    }
    private void Update()
    {
        label.text = $"{electricityReceived} / {houseSO.electricityNeeded}";
    }

    public void DeleteButton()
    {
        playerInfo.addMoney(houseSO.price * .8f);
        // ! Might change this in the future to the exact same system as the electricity
        playerInfo.addPopulation(-houseSO.population);
        playerInfo.UseElectricity(-houseSO.electricityNeeded);
        playerInfo.useWater(-houseSO.waterNeeded);

        Destroy(gameObject);
    }
    public void Deactivate(int electricityLost)
    {
        Debug.Log("Deactivated");
        elecMissingImage.color = Color.red;
        isActive = false;
        electricityReceived -= electricityLost;
        playerInfo.addPopulation(-houseSO.population);
    }
    public void Activate()
    {
        elecMissingImage.color = Color.green;
        isActive = true;
        playerInfo.addPopulation(houseSO.population);
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        playerInfo.UseElectricity(electricity);
    }

    public IBuildingSO GetBuildingSO() => houseSO;
    public string GetBuildingType() => houseSO.type;
    public string GetRessource() => houseSO.ressource;
    public int GetPopulation() => houseSO.population;
    public int GetPrice() => houseSO.price;
    public int GetElectricityReceived() => electricityReceived;
    public int GetElectricityNeeded() => houseSO.electricityNeeded;
    public bool CanPlace() =>
        playerInfo.getMoney >= houseSO.price &&
        playerInfo.getAvailableElectricity >= houseSO.electricityNeeded &&
        playerInfo.getAvailableWater >= houseSO.waterNeeded;

    public bool IsActive() => isActive;
    public bool HasEnoughElectricity() => electricityReceived >= houseSO.electricityNeeded;
}