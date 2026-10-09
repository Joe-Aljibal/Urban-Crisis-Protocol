using UnityEngine;

[CreateAssetMenu(fileName = "Water Silo", menuName = "ScriptableObjects/Water Silo")]
public class WaterSO : ScriptableObject, IBuildingSO
{
    public string type = "Utilities";
    public string ressource = "Water";
    public int electricityNeeded = 0;
    public int water = 10;
    public int price = 25;
    public BuildingCard card;
    public Sprite icon;


    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney;
}
