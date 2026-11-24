using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Cree l'UI du menu pause dans la scene ouverte (MainGame) et la branche au PauseMenu :
// panneau Settings, panneau de confirmation Quit et resume de la partie dans MenuBreak.
// Les panneaux copient le fond de MenuBreak et le style du bouton Resume : a restyler ensuite si besoin.
// Relancer le menu remplace les anciens elements.
public static class SettingsPanelBuilder
{
    const string SettingsName = "SettingsPanel";
    const string ConfirmQuitName = "ConfirmQuitPanel";
    const string SummaryName = "Summary";

    [MenuItem("Tools/Urban Crisis/Build Pause Menu UI")]
    static void BuildInOpenScene()
    {
        if (Build())
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // Unity.exe -batchmode -quit -projectPath ... -executeMethod SettingsPanelBuilder.BuildFromCommandLine
    public static void BuildFromCommandLine()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
        if (Build())
            EditorSceneManager.SaveScene(scene);
    }

    static bool Build()
    {
        var pauseMenu = Object.FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);
        if (pauseMenu == null)
        {
            Debug.LogError("[Settings] Aucun PauseMenu dans la scene ouverte (ouvrir MainGame).");
            return false;
        }

        var pauseMenuSO = new SerializedObject(pauseMenu);
        var pausePanel = (GameObject)pauseMenuSO.FindProperty("pausePanel").objectReferenceValue;

        Button resumeButton = FindButton(pausePanel, "Resume");
        Button settingButton = FindButton(pausePanel, "Setting");
        TMP_Text textTemplate = resumeButton.GetComponentInChildren<TMP_Text>(true);

        // Le resume est enleve avant de copier MenuBreak pour les autres panneaux.
        Transform oldSummary = pausePanel.transform.Find(SummaryName);
        if (oldSummary != null)
            Undo.DestroyObjectImmediate(oldSummary.gameObject);

        GameObject settingsPanel = BuildSettingsPanel(pauseMenu, pausePanel, resumeButton, textTemplate);
        GameObject confirmPanel = BuildConfirmQuitPanel(pauseMenu, pausePanel, resumeButton, textTemplate);
        TMP_Text summary = BuildSummary(pausePanel, textTemplate);

        pauseMenuSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        pauseMenuSO.FindProperty("confirmQuitPanel").objectReferenceValue = confirmPanel;
        pauseMenuSO.FindProperty("summaryText").objectReferenceValue = summary;
        pauseMenuSO.ApplyModifiedProperties();

        if (!HasListener(settingButton, pauseMenu, nameof(PauseMenu.OpenSettings)))
        {
            Undo.RecordObject(settingButton, "Wire Setting button");
            UnityEventTools.AddPersistentListener(settingButton.onClick, pauseMenu.OpenSettings);
        }

        Debug.Log("[Settings] UI du menu pause creee et branchee (Settings, confirmation Quit, resume).");
        return true;
    }

    static GameObject BuildSettingsPanel(PauseMenu pauseMenu, GameObject pausePanel, Button buttonTemplate, TMP_Text textTemplate)
    {
        GameObject panel = CreatePanel(pausePanel, SettingsName);

        TMP_Text title = CreateText(textTemplate, panel.transform, "Title", "Settings", 36, TextAlignmentOptions.Center);
        SetHeight(title.gameObject, 50);

        (Slider masterSlider, TMP_Text masterValue) = CreateSliderRow(textTemplate, panel.transform, "MasterVolume", "Volume");
        (Slider musicSlider, TMP_Text musicValue) = CreateSliderRow(textTemplate, panel.transform, "MusicVolume", "Music");

        CreateButton(buttonTemplate, panel.transform, "Back", pauseMenu.CloseSettings);

        var settingsMenu = panel.AddComponent<SettingsMenu>();
        var settingsSO = new SerializedObject(settingsMenu);
        settingsSO.FindProperty("masterSlider").objectReferenceValue = masterSlider;
        settingsSO.FindProperty("masterValue").objectReferenceValue = masterValue;
        settingsSO.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        settingsSO.FindProperty("musicValue").objectReferenceValue = musicValue;
        settingsSO.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
        return panel;
    }

    static GameObject BuildConfirmQuitPanel(PauseMenu pauseMenu, GameObject pausePanel, Button buttonTemplate, TMP_Text textTemplate)
    {
        GameObject panel = CreatePanel(pausePanel, ConfirmQuitName);

        TMP_Text title = CreateText(textTemplate, panel.transform, "Title", "Quit to home screen?", 36, TextAlignmentOptions.Center);
        SetHeight(title.gameObject, 50);
        TMP_Text warning = CreateText(textTemplate, panel.transform, "Warning", "Your city will not be saved.", 24, TextAlignmentOptions.Center);
        SetHeight(warning.gameObject, 40);

        var row = new GameObject("Buttons", typeof(RectTransform));
        row.transform.SetParent(panel.transform, false);
        SetHeight(row, ((RectTransform)buttonTemplate.transform).rect.height);
        var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 60;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        CreateButton(buttonTemplate, row.transform, "Yes", pauseMenu.ConfirmQuit);
        CreateButton(buttonTemplate, row.transform, "No", pauseMenu.CancelQuit);

        panel.SetActive(false);
        return panel;
    }

    // Texte au centre de MenuBreak, au-dessus des boutons (en bas de l'ecran).
    static TMP_Text BuildSummary(GameObject pausePanel, TMP_Text textTemplate)
    {
        TMP_Text summary = CreateText(textTemplate, pausePanel.transform, SummaryName, "Money: 0 $", 28, TextAlignmentOptions.Center);
        summary.textWrappingMode = TextWrappingModes.Normal;

        var rect = summary.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 20);
        rect.sizeDelta = new Vector2(500, 220);
        Undo.RegisterCreatedObjectUndo(summary.gameObject, "Build Pause Summary");
        return summary;
    }

    // Copie vide de MenuBreak (meme fond, meme taille) avec une mise en page verticale.
    static GameObject CreatePanel(GameObject pausePanel, string name)
    {
        Transform parent = pausePanel.transform.parent;
        Transform old = parent.Find(name);
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);

        GameObject panel = Object.Instantiate(pausePanel, parent);
        panel.name = name;
        panel.SetActive(true);
        panel.transform.SetSiblingIndex(pausePanel.transform.GetSiblingIndex() + 1);
        for (int i = panel.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panel.transform.GetChild(i).gameObject);
        foreach (LayoutGroup group in panel.GetComponents<LayoutGroup>())
            Object.DestroyImmediate(group);
        Undo.RegisterCreatedObjectUndo(panel, "Build " + name);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 30, 30);
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return panel;
    }

    // Copie du bouton Resume, sans ses anciens evenements.
    static Button CreateButton(Button template, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        Button button = Object.Instantiate(template, parent);
        button.name = label;
        button.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(button.onClick, onClick);
        button.GetComponentInChildren<TMP_Text>(true).text = label;

        var element = button.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = ((RectTransform)template.transform).rect.height;
        element.preferredWidth = ((RectTransform)template.transform).rect.width;
        return button;
    }

    static (Slider, TMP_Text) CreateSliderRow(TMP_Text textTemplate, Transform parent, string name, string label)
    {
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        SetHeight(row, 40);

        var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 15;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        TMP_Text labelText = CreateText(textTemplate, row.transform, "Label", label, 24, TextAlignmentOptions.Left);
        labelText.gameObject.AddComponent<LayoutElement>().preferredWidth = 120;

        var resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
        };
        Slider slider = DefaultControls.CreateSlider(resources).GetComponent<Slider>();
        slider.name = "Slider";
        slider.transform.SetParent(row.transform, false);
        slider.minValue = 0;
        slider.maxValue = 1;
        var sliderLayout = slider.gameObject.AddComponent<LayoutElement>();
        sliderLayout.flexibleWidth = 1;
        sliderLayout.preferredHeight = 20;

        TMP_Text valueText = CreateText(textTemplate, row.transform, "Value", "100 %", 24, TextAlignmentOptions.Right);
        valueText.gameObject.AddComponent<LayoutElement>().preferredWidth = 80;

        return (slider, valueText);
    }

    static TMP_Text CreateText(TMP_Text template, Transform parent, string name, string text, float size, TextAlignmentOptions alignment)
    {
        TMP_Text copy = Object.Instantiate(template, parent);
        copy.name = name;
        copy.text = text;
        copy.fontSize = size;
        copy.enableAutoSizing = false;
        copy.alignment = alignment;
        copy.textWrappingMode = TextWrappingModes.NoWrap;
        return copy;
    }

    static void SetHeight(GameObject go, float height)
    {
        if (!go.TryGetComponent(out LayoutElement element))
            element = go.AddComponent<LayoutElement>();
        element.preferredHeight = height;
    }

    static Button FindButton(GameObject panel, string name)
    {
        foreach (Button button in panel.GetComponentsInChildren<Button>(true))
            if (button.name == name)
                return button;

        throw new System.Exception($"[Settings] Bouton '{name}' introuvable dans {panel.name}.");
    }

    static bool HasListener(Button button, Object target, string method)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            if (button.onClick.GetPersistentTarget(i) == target && button.onClick.GetPersistentMethodName(i) == method)
                return true;
        return false;
    }
}
