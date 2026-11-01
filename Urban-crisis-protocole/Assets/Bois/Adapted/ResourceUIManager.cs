using System;
using TMPro;
using UnityEngine;

public class ResourceUIManager : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    private void UpdateWoodText()
    {
        woodText.text = $"Wood: {playerInfo.GetWood}";
    }
    private void UpdateStoneText()
    {
        stoneText.text = $"Stone: {playerInfo.GetStone}";
    }
    void Update()
    {
        UpdateWoodText();
        UpdateStoneText();
    }
}