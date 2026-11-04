using UnityEngine;
using UnityEngine.UIElements;

public class GeneralScriptUI : MonoBehaviour
{
    PanelRenderer panelRenderer;
    VisualElement root;

    void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(SettingUp);
    }

  void SettingUp(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        root = rootElement;
    }

    void UpdateStatsUI()
    {
        PlayerInfo playerInfo = PlayerInfo.Instance;
        Label moneyText = root.Q<Label>("MoneyValue");
        Label populationText = root.Q<Label>("PopulationValue");
        Label electricityText = root.Q<Label>("ElectricityValue");
        Label waterText = root.Q<Label>("WaterValue");
        Label researchText = root.Q<Label>("FoodValue");
        Label researchText2 = root.Q<Label>("ResearchPoints");

        moneyText.text = $"${playerInfo.GetMoney}";
        populationText.text = $"<color=white>{playerInfo.GetWorkingPopulation}</color> / {playerInfo.GetPopulation}";
        electricityText.text = $"<color=white>{playerInfo.GetElectricityUsed}</color> / {playerInfo.GetElectricity}";
        waterText.text = $"<color=white>{playerInfo.GetWaterUsed}</color> / {playerInfo.GetWater}";
        researchText.text = $"{playerInfo.GetFood}";
        researchText2.text = $"🧪 {playerInfo.GetResearchPoints()}";
    }

    void Update()
    {
        UpdateStatsUI();
    }
}
