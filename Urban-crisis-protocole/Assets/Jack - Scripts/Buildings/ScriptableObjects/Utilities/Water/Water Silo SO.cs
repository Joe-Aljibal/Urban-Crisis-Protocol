using UnityEngine;

[CreateAssetMenu(fileName = "Water Silo", menuName = "ScriptableObjects/Water Silo")]
public class WaterSiloSO : ScriptableObject, IBuildingSO
{
    public string type = "Utilities";
    public string ressource = "Water";
    public int electricityNeeded = 0;
    public int water = 10;
    public int price = 25;
}
