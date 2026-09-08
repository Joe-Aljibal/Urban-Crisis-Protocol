using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class SolarPanel : MonoBehaviour, IBuilding
{
    [SerializeField] Canvas popUp;
    [SerializeField] public ElectricitySO solarPanelSO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    bool editing = false;

    private void Start()
    {
        playerInfo.addElectricity(solarPanelSO.electricityOutput);
    }

    private void Update()
    {
        //popUp.enabled = editing;
        if (Keyboard.current.xKey.wasPressedThisFrame)
            editing = false;
    }


    public void DeleteButton()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        playerInfo.addElectricity(-solarPanelSO.electricityOutput);
    }

    public IBuildingSO getBuildingSO() => solarPanelSO;
    public string getType() => solarPanelSO.type;
    public string getRessource() => solarPanelSO.ressource;
    public int getElectricityNeeded() => 0;
    public int getPrice() => solarPanelSO.price;
    public bool canPlace() => playerInfo.getMoney >= solarPanelSO.price;
}
