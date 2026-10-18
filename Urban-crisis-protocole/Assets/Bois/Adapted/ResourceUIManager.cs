using TMPro;
using UnityEngine;

public class ResourceUIManager : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;

    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    [SerializeField] private bool showSimpleCounter;

    private void UpdateWoodText()
    {
        woodText.text = $"Wood: {playerInfo.getWood}";
    }
    private void UpdateStoneText()
    {
        stoneText.text = $"Stone: {playerInfo.getStone}";
    }
    private void Update()
    {
        if (showSimpleCounter)
            return;

        UpdateWoodText();
        UpdateStoneText();
    }

    private void OnGUI()
    {
        if (!showSimpleCounter)
            return;

        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 18;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;

        GUIStyle sectionStyle = new GUIStyle(labelStyle);
        sectionStyle.fontStyle = FontStyle.Bold;

        GUILayout.BeginArea(
            new Rect(20f, 20f, 340f, 265f),
            "STATISTIQUES DES RESSOURCES",
            windowStyle
        );

        GUILayout.Space(14f);
        GUILayout.Label("Stock actuel", sectionStyle);
        GUILayout.Label("Bois : " + playerInfo.getWood, labelStyle);
        GUILayout.Label("Pierre : " + playerInfo.getStone, labelStyle);
        GUILayout.Label("Charbon : " + playerInfo.getCoal, labelStyle);

        GUILayout.Space(10f);
        GUILayout.Label("Ressources exploitées", sectionStyle);
        GUILayout.Label("Bois : " + playerInfo.getExploitedWood, labelStyle);
        GUILayout.Label("Pierre : " + playerInfo.getExploitedStone, labelStyle);
        GUILayout.Label("Charbon : " + playerInfo.getExploitedCoal, labelStyle);
        GUILayout.Label("Total : " + playerInfo.getTotalExploited, sectionStyle);
        GUILayout.EndArea();
    }
}
