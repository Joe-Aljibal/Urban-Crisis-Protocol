using TMPro;
using UnityEngine;

public class ResourceUIManager : MonoBehaviour
{
    private const int WindowFontSize = 18;
    private const int LabelFontSize = 16;
    private const float DashboardLeft = 20f;
    private const float DashboardTop = 20f;
    private const float DashboardWidth = 400f;
    private const float MaximumDashboardHeight = 520f;
    private const float DashboardVerticalMargin = 40f;
    private const float TitleSpacing = 14f;
    private const float CollectorSpacing = 6f;
    private const float DebugButtonHeight = 35f;
    private const int MinutesPerHour = 60;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;

    [SerializeField] private bool showSimpleCounter;
    [SerializeField] private SimpleResourceCollector resourceCollector;
    [SerializeField] private SimpleResourceCollector aiResourceCollector;
    [SerializeField] private DayNightCycle dayNightCycle;

    private Vector2 dashboardScroll;

    public static bool IsPointerOverDashboard { get; private set; }

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
        {
            IsPointerOverDashboard = false;
            return;
        }

        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = WindowFontSize;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = LabelFontSize;

        GUIStyle sectionStyle = new GUIStyle(labelStyle);
        sectionStyle.fontStyle = FontStyle.Bold;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = LabelFontSize;

        float dashboardHeight = Screen.height - DashboardVerticalMargin;

        if (dashboardHeight > MaximumDashboardHeight)
        {
            dashboardHeight = MaximumDashboardHeight;
        }

        Rect dashboardArea = new Rect(
            DashboardLeft,
            DashboardTop,
            DashboardWidth,
            dashboardHeight
        );
        IsPointerOverDashboard = dashboardArea.Contains(Event.current.mousePosition);

        GUILayout.BeginArea(
            dashboardArea,
            "DASHBOARD DES RESSOURCES",
            windowStyle
        );

        GUILayout.Space(TitleSpacing);
        dashboardScroll = GUILayout.BeginScrollView(dashboardScroll);

        DrawCurrentTime(sectionStyle, labelStyle, buttonStyle);
        DrawPlayerStock(sectionStyle, labelStyle);
        DrawTownResources(TownType.Human, "VILLE HUMAINE", sectionStyle, labelStyle);
        DrawTownResources(TownType.AI, "VILLE IA", sectionStyle, labelStyle);
        DrawComparison(sectionStyle, labelStyle);
        DrawCollectors(sectionStyle, labelStyle);

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawCurrentTime(
        GUIStyle sectionStyle,
        GUIStyle labelStyle,
        GUIStyle buttonStyle
    )
    {
        float currentHour = dayNightCycle.GetCurrentHour();
        int hour = Mathf.FloorToInt(currentHour);
        int minute = Mathf.FloorToInt(
            (currentHour - hour) * MinutesPerHour
        );

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("CYCLE JOUR / NUIT", sectionStyle);
        GUILayout.Label($"Heure actuelle : {hour:00}:{minute:00}", labelStyle);

        string buttonText =
            $"Avancer de {dayNightCycle.GetDebugTimeJump():0.#} h (Espace)";

        if (GUILayout.Button(
            buttonText,
            buttonStyle,
            GUILayout.Height(DebugButtonHeight)
        ))
        {
            dayNightCycle.AdvanceTime();
        }

        GUILayout.EndVertical();
    }

    private void DrawPlayerStock(GUIStyle sectionStyle, GUIStyle labelStyle)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("STOCK HUMAIN ACTUEL", sectionStyle);
        GUILayout.Label("Bois : " + playerInfo.getWood, labelStyle);
        GUILayout.Label("Pierre : " + playerInfo.getStone, labelStyle);
        GUILayout.Label("Charbon : " + playerInfo.getCoal, labelStyle);
        GUILayout.EndVertical();
    }

    private void DrawTownResources(
        TownType town,
        string title,
        GUIStyle sectionStyle,
        GUIStyle labelStyle
    )
    {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(title + " - RESSOURCES EXPLOITÉES", sectionStyle);
        GUILayout.Label("Bois : " + TownResourceStatistics.GetWood(town), labelStyle);
        GUILayout.Label("Pierre : " + TownResourceStatistics.GetStone(town), labelStyle);
        GUILayout.Label("Total : " + TownResourceStatistics.GetTotal(town), sectionStyle);
        GUILayout.EndVertical();
    }

    private void DrawComparison(GUIStyle sectionStyle, GUIStyle labelStyle)
    {
        int humanTotal = TownResourceStatistics.GetTotal(TownType.Human);
        int aiTotal = TownResourceStatistics.GetTotal(TownType.AI);

        string leader = "Égalité";

        if (humanTotal > aiTotal)
            leader = "Avantage : ville humaine";
        else if (aiTotal > humanTotal)
            leader = "Avantage : ville IA";

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("COMPARAISON", sectionStyle);
        GUILayout.Label("Humains : " + humanTotal, labelStyle);
        GUILayout.Label("IA : " + aiTotal, labelStyle);
        GUILayout.Label(leader, sectionStyle);
        GUILayout.EndVertical();
    }

    private void DrawCollectors(GUIStyle sectionStyle, GUIStyle labelStyle)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("NPC EXPLOITEURS", sectionStyle);
        GUILayout.Label("Humain : " + resourceCollector.CurrentAction, labelStyle);
        GUILayout.Label("Livraisons : " + resourceCollector.DeliveredResources, labelStyle);
        GUILayout.Space(CollectorSpacing);
        GUILayout.Label("IA : " + aiResourceCollector.CurrentAction, labelStyle);
        GUILayout.Label("Livraisons : " + aiResourceCollector.DeliveredResources, labelStyle);
        GUILayout.EndVertical();
    }
}
