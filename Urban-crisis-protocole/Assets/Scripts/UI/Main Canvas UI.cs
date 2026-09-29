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
    void Update()
    {
        moneyText.text = "Money: "  + playerInfo.getMoney.ToString();
        electricityText.text =  "Electricity: " + playerInfo.getElectricity.ToString() + $" <color=yellow> {playerInfo.getElectricityUsed}</color>";
        populationText.text = "Population: " + playerInfo.getPopulation.ToString() + $" <color=red> {playerInfo.getWorkingPopulation}</color>";
        waterText.text = "Water: " + playerInfo.getWater.ToString() + $" <color=blue> {playerInfo.getWaterUsed}</color>";
    }
}
