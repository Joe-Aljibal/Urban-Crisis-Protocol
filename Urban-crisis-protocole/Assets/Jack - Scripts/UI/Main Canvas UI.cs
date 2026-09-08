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
        moneyText.text = playerInfo.getMoney.ToString();
        electricityText.text = playerInfo.getElectricity.ToString() + $" <color=yellow> {playerInfo.getElectricityUsed}</color>";
        populationText.text = playerInfo.getPopulation.ToString() + $" <color=red> {playerInfo.getWorkingPopulation}</color>";
        waterText.text = playerInfo.getWater.ToString() + $" <color=blue> {playerInfo.getWaterUsed}</color>";
    }
}
