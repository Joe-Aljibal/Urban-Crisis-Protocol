using TMPro;
using UnityEngine;

// Affiche les ressources du joueur dans les compteurs de MainGame.
public class HudController : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyCount;
    [SerializeField] private TMP_Text residentCount;
    [SerializeField] private TMP_Text electricityCount;
    [SerializeField] private TMP_Text waterCount;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    void Update()
    {
        moneyCount.text = playerInfo.FormattedMoney + " $";

        // Habitants disponibles / habitants total (les autres travaillent)
        residentCount.text = playerInfo.getAvailablePopulation + " / " + playerInfo.getPopulation;

        // Utilise / produit
        electricityCount.text = playerInfo.getElectricityUsed + " / " + playerInfo.getElectricity;
        waterCount.text = playerInfo.getWaterUsed + " / " + playerInfo.getWater;
    }
}
