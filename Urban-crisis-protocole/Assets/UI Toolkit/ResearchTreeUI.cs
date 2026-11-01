using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchTreeUI : MonoBehaviour
{
    PanelRenderer panelRenderer;
    VisualElement root;
    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(ResearchPageUI);
    }

    void ResearchPageUI(PanelRenderer renderer, VisualElement rootElement, int version)
    {
        root = rootElement;
        root.Q<Button>("close-button").clicked += () => CloseButton();
    }

    void CloseButton()
    {
        VisualElement treePage = root.Q("skill-tree-root");

        treePage.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        treePage.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        treePage.style.translate = new Translate(0, 0);
    }

}
