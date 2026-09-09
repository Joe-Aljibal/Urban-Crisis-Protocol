using System;
using TMPro;
using UnityEngine;

public class ResourceUIManager : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    [SerializeField] private TMP_Text woodText;

    private void UpdateWoodText()
    {
        woodText.text = $"Wood: {playerInfo.getWood}";
    }
    void Update()
    {
        UpdateWoodText();
    }
}