using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        PlayerInfo.Instance.addStat += AnimateStat;
    }

    void OnDestroy()
    {
        PlayerInfo.Instance.addStat -= AnimateStat;
        if (panelRenderer != null) panelRenderer.UnregisterUIReloadCallback(SettingUp);
    }

    void SettingUp(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        root = rootElement;
    }

    public void AnimateStat(string labelName, string statValue)
    {
        if (root == null) return;
        Label label = root.Q<Label>(labelName);
        if (label == null || label.parent == null) return;
        Label newLabel = new Label(statValue);

        newLabel.style.position = Position.Absolute;
        newLabel.style.fontSize = new StyleLength(20);
        switch (labelName) {
            case "PopulationValue":
                newLabel.style.color = Color.blue;
                break;
            case "ElectricityValue":
                newLabel.style.color = Color.yellow;
                break;
            case "WaterValue":
                newLabel.style.color = Color.aquamarine;
                break;
            case "MoneyValue":
                newLabel.style.color = Color.green;
                break;
        }

        newLabel.style.translate = new Translate(0, 0);
        newLabel.style.opacity = 1f;

        newLabel.style.transitionProperty = new List<StylePropertyName>
    {
        new StylePropertyName("translate"),
        new StylePropertyName("opacity")
    };
        newLabel.style.transitionDuration = new List<TimeValue>
    {
        new TimeValue(500, TimeUnit.Millisecond),
        new TimeValue(500, TimeUnit.Millisecond)
    };
        newLabel.style.transitionTimingFunction = new List<EasingFunction>
    {
        EasingMode.EaseOut,
        EasingMode.Linear
    };

        newLabel.RegisterCallback<TransitionEndEvent>(evt =>
        {
            if (evt.stylePropertyNames.Contains(new StylePropertyName("opacity")))
                newLabel.RemoveFromHierarchy();
        });

        label.parent.Add(newLabel);

        newLabel.schedule.Execute(() =>
        {
            newLabel.style.translate = new Translate(0, -150);
            newLabel.style.opacity = 0f;
        }).StartingIn(10);
    }

    void UpdateStatsUI()
    {
        if (root == null) return;
        PlayerInfo playerInfo = PlayerInfo.Instance;
        Label moneyText = root.Q<Label>("MoneyValue");
        Label populationText = root.Q<Label>("PopulationValue");
        Label electricityText = root.Q<Label>("ElectricityValue");
        Label waterText = root.Q<Label>("WaterValue");
        Label researchText = root.Q<Label>("ResearchPointsValue");
        Label researchText2 = root.Q<Label>("ResearchPoints");

        if (moneyText != null) moneyText.text = $"${playerInfo.FormattedMoney}";
        if (populationText != null) populationText.text = $"<color=white>{playerInfo.GetWorkingPopulation}</color> / {playerInfo.GetPopulation}";
        if (electricityText != null) electricityText.text = $"<color=white>{playerInfo.GetElectricityUsed}</color> / {playerInfo.GetElectricity}";
        if (waterText != null) waterText.text = $"<color=white>{playerInfo.GetWaterUsed}</color> / {playerInfo.GetWater}";
        if (researchText != null) researchText.text = $"{playerInfo.GetResearchPoints()}";
        if (researchText2 != null) researchText2.text = $"🧪 {playerInfo.GetResearchPoints()}";
    }

    void Update()
    {
        UpdateStatsUI();
    }
}
