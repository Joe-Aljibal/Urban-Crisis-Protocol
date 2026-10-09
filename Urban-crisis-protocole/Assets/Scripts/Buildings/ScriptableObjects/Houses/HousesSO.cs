using UnityEngine;

[CreateAssetMenu(fileName = "House", menuName = ("ScriptableObjects/House"))]
public class HousesSO : ScriptableObject, IBuildingSO
{
    public string type = "Population";
    public string ressource = "Population";
    public int electricityNeeded;
    public int waterNeeded;
    public int population;
    public int price;
    public BuildingCard card;
    public Sprite icon;

    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;

    PlayerInfo playerInfo = PlayerInfo.Instance;
     public bool CanPlace() =>
        playerInfo.getMoney >= price &&
        playerInfo.getAvailableElectricity >= electricityNeeded;  // !! water 
}
