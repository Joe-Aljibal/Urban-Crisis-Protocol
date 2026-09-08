using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ElectricitySO", menuName = "ScriptableObjects/ElectricitySO")]
public class ElectricitySO : ScriptableObject, IBuildingSO
{
    public string type = "Utility";
    public string ressource = "Electricity";
    public int electricityOutput;
    public int electricity = 0;
    public int price;

    public void AddElectricity()
    {
        PlayerInfo.Instance.addElectricity(electricityOutput);
    }
}
