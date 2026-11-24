using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingDescriptionUI : MonoBehaviour
{
    ViewModelCam viewModelCamera;
    [SerializeField] GameObject viewModelPivot;
    Coroutine hideDescCoroutine;

    private void Awake()
    {
        viewModelCamera = FindAnyObjectByType<ViewModelCam>();
    }

    public void ShowDescription(VisualElement rootElement, IBuildingSO so)
    {
        if (hideDescCoroutine != null) StopCoroutine(hideDescCoroutine);

        Camera cam = GameObject.Find("3DModelViewCam").GetComponent<Camera>();
        // Handling 3d view model
        viewModelCamera.StartSpinning();
        foreach (Transform child in viewModelPivot.transform) { Destroy(child.gameObject); }
        GameObject model = Instantiate(so.GetCard().meshPrefab, Vector3.zero, Quaternion.identity, viewModelPivot.transform);
        model.layer = LayerMask.NameToLayer("3DModelView");
        cam.GetComponent<ViewModelCam>().Frame(model);
        foreach (Transform child in model.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("3DModelView");
        }

        VisualElement descWindow = rootElement.Q("BuildingInfo");

        //descWindow.Q("ModelContainer").style.backgroundImage = new StyleBackground(so.GetIcon());
        descWindow.Q<Label>("BuildingName").text = so.GetBuildingName();
        descWindow.Q<Label>("BuildingDescription").text = so.GetDescription();
        var buildingStats = so.GetStats();

        rootElement.Q("PopUpGrid").Clear();
        foreach (var stat in buildingStats)
        {
            CreatePopUpGridElement(rootElement, stat);
        }

        descWindow.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        descWindow.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
        descWindow.style.translate = new Translate(0, -460);
        descWindow.style.scale = new Scale(new Vector2(1, 1));

        hideDescriptionCooldown = 0;
    }

    VisualElement CreatePopUpGridElement(VisualElement rootElement, Dictionary<string, string> stat)
    {
        VisualElement parent = new VisualElement();
        parent.AddToClassList("info-tile");

        Label icon = new Label(stat["icon"]);
        icon.AddToClassList("tile-icon");

        Label label = new Label(stat["label"]);
        label.AddToClassList("tile-label");

        Label value = new Label(stat["value"]);
        value.AddToClassList("tile-value");

        parent.Add(icon);
        parent.Add(label);
        parent.Add(value);
        rootElement.Q("PopUpGrid").Add(parent);

        return parent;
    }

    float hideDescriptionCooldown = 0;

    public void HideDescription(VisualElement rootElement)
    {
        hideDescCoroutine = StartCoroutine(HideDesc(rootElement));
    }

    IEnumerator HideDesc(VisualElement rootElement)
    {
        if (hideDescriptionCooldown < 1f)
        {
            hideDescriptionCooldown += Time.deltaTime;
            yield return null;
        }
        hideDescriptionCooldown = 0;

        VisualElement descWindow = rootElement.Q("BuildingInfo");

        descWindow.style.transitionDuration = new List<TimeValue> { new TimeValue(150, TimeUnit.Millisecond) };
        descWindow.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("scale") };
        descWindow.style.translate = new Translate(0, 0);
        descWindow.style.scale = new Scale(new Vector2(0, 0));

        viewModelCamera.StopSpinning();
    }
}
