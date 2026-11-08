using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainCanvasUI : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text electricityText;
    [SerializeField] TMP_Text populationText;
    [SerializeField] TMP_Text waterText;
    PlayerInfo playerInfo = PlayerInfo.Instance;
    //void Update()
    //{
    //    moneyText.text = "Money: "  + playerInfo.GetMoney.ToString();
    //    electricityText.text =  "Electricity: " + playerInfo.GetElectricity.ToString() + $" <color=yellow> {playerInfo.GetElectricityUsed}</color>";
    //    populationText.text = "Population: " + playerInfo.GetPopulation.ToString() + $" <color=red> {playerInfo.GetWorkingPopulation}</color>";
    //    waterText.text = "Water: " + playerInfo.GetWater.ToString() + $" <color=blue> {playerInfo.GetWaterUsed}</color>";
    //}
}
