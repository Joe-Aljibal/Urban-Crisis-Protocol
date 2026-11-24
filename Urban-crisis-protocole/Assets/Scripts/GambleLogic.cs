using UnityEngine;

public class GambleLogic : MonoBehaviour
{
    private readonly PlayerInfo playerInfo = PlayerInfo.Instance;
    private GamblingPanelUI panel;

    public void OpenPanel()
    {
        if (panel == null)
            panel = GamblingPanelUI.Create(this);

        panel.Show(playerInfo.GetMoney);
    }

    public float GetMoney() => playerInfo.GetMoney;

    public float Gamble(bool won)
    {
        float currentMoney = playerInfo.GetMoney;
        float amount = currentMoney * 0.20f * (won ? 1f : -1f);

        playerInfo.AddMoney(amount);
        if (panel != null)
            panel.ShowResult(playerInfo.GetMoney, amount);
        return amount;
    }
}
