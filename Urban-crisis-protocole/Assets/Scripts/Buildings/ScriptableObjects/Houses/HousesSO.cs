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
}
