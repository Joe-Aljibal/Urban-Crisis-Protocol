using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Cree le panneau Settings du menu pause dans la scene ouverte (MainGame) et le branche.
// Le panneau copie le fond de MenuBreak et le style du bouton Resume : a restyler ensuite si besoin.
// Relancer le menu remplace l'ancien panneau.
public static class SettingsPanelBuilder
{
    const string PanelName = "SettingsPanel";

    [MenuItem("Tools/Urban Crisis/Build Settings Panel")]
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
        Transform parent = pausePanel.transform.parent;

        Button resumeButton = FindButton(pausePanel, "Resume");
        Button settingButton = FindButton(pausePanel, "Setting");
        TMP_Text textTemplate = resumeButton.GetComponentInChildren<TMP_Text>(true);

        Transform old = parent.Find(PanelName);
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);

        // Meme fond et meme taille que MenuBreak, sans ses boutons.
        GameObject panel = Object.Instantiate(pausePanel, parent);
        panel.name = PanelName;
        panel.transform.SetSiblingIndex(pausePanel.transform.GetSiblingIndex() + 1);
        for (int i = panel.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panel.transform.GetChild(i).gameObject);
        foreach (LayoutGroup group in panel.GetComponents<LayoutGroup>())
            Object.DestroyImmediate(group);
        Undo.RegisterCreatedObjectUndo(panel, "Build Settings Panel");

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 30, 30);
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text title = CreateText(textTemplate, panel.transform, "Title", "Settings", 36, TextAlignmentOptions.Center);
        SetHeight(title.gameObject, 50);

        (Slider masterSlider, TMP_Text masterValue) = CreateSliderRow(textTemplate, panel.transform, "MasterVolume", "Volume");
        (Slider musicSlider, TMP_Text musicValue) = CreateSliderRow(textTemplate, panel.transform, "MusicVolume", "Music");

        // Bouton Back : copie de Resume, sans ses anciens evenements.
        Button back = Object.Instantiate(resumeButton, panel.transform);
        back.name = "Back";
        back.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(back.onClick, pauseMenu.CloseSettings);
        back.GetComponentInChildren<TMP_Text>(true).text = "Back";
        var backLayout = back.gameObject.AddComponent<LayoutElement>();
        backLayout.preferredHeight = ((RectTransform)resumeButton.transform).rect.height;
        backLayout.preferredWidth = ((RectTransform)resumeButton.transform).rect.width;

        var settingsMenu = panel.AddComponent<SettingsMenu>();
        var settingsSO = new SerializedObject(settingsMenu);
        settingsSO.FindProperty("masterSlider").objectReferenceValue = masterSlider;
        settingsSO.FindProperty("masterValue").objectReferenceValue = masterValue;
        settingsSO.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        settingsSO.FindProperty("musicValue").objectReferenceValue = musicValue;
        settingsSO.ApplyModifiedPropertiesWithoutUndo();

        pauseMenuSO.FindProperty("settingsPanel").objectReferenceValue = panel;
        pauseMenuSO.ApplyModifiedProperties();

        if (!HasListener(settingButton, pauseMenu, nameof(PauseMenu.OpenSettings)))
        {
            Undo.RecordObject(settingButton, "Wire Setting button");
            UnityEventTools.AddPersistentListener(settingButton.onClick, pauseMenu.OpenSettings);
        }

        panel.SetActive(false);
        Debug.Log("[Settings] Panneau Settings cree et branche au menu pause.");
        return true;
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
