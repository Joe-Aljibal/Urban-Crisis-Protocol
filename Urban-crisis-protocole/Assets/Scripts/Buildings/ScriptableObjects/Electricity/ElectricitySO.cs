using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ElectricitySO", menuName = "ScriptableObjects/ElectricitySO")]
public class ElectricitySO : ScriptableObject, IBuildingSO
{
    public string type = "Utility";
    public string ressource = "Electricity";
    public int electricityOutput;
    public int price;
    public BuildingCard card;
    public Sprite icon;

    public Sprite GetIcon() => icon;
    public BuildingCard GetCard() => card;
    public bool CanPlace() => price <= PlayerInfo.Instance.getMoney;
}
