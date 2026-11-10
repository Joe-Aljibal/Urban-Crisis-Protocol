using UnityEngine;
using UnityEngine.InputSystem;

// Uses the original building cards and placement manager in NewWorldDesign.
public class CityConstruction : MonoBehaviour
{
    [SerializeField] private BuildingCard[] cards;
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    private BuildingManager manager;
    private GameObject constructionRoot;
    public Vector3 BuildAreaPosition => constructionRoot != null ? constructionRoot.transform.position : new Vector3(-120f, 0f, -55f);
    private GameObject preview;
    private static CityConstruction instance;
    private bool menuOpen;
    private int selectedIndex;
    private static Rect Panel => new Rect(20f, Mathf.Min(ResourceUIManager.PanelBottom + 8f, Screen.height - 120f), Mathf.Min(320f, Screen.width - 40f), Mathf.Max(110f, Mathf.Min(200f, Screen.height - ResourceUIManager.PanelBottom - 28f)));
    public static bool IsMenuOpen => instance != null && instance.menuOpen;
    private static Rect Tab => new Rect(20f, Mathf.Min(ResourceUIManager.PanelBottom + 8f, Screen.height - 95f), 205f, 40f);
    public static bool IsPointerOverPanel => instance != null && !CityFireEvent.IsMenuOpen && Mouse.current != null &&
        (instance.menuOpen ? Panel : Tab).Contains(new Vector2(Mouse.current.position.ReadValue().x, Screen.height - Mouse.current.position.ReadValue().y));

    private void Start()
    {
        instance = this;
        constructionRoot = new GameObject("Construction area");
        constructionRoot.transform.position = new Vector3(-120f, 0.15f, -55f);
        GameObject buildArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buildArea.name = "Build area";
        buildArea.transform.SetParent(constructionRoot.transform, false);
        buildArea.transform.localPosition = new Vector3(0, -0.12f, 0);
        buildArea.transform.localScale = new Vector3(30f, 0.04f, 30f);
        Destroy(buildArea.GetComponent<Collider>());
        buildArea.GetComponent<Renderer>().material.color = new Color(0.42f, 0.5f, 0.38f);
        Grid grid = constructionRoot.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f, 0.5f, 0.5f);
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        BoxCollider ground = constructionRoot.AddComponent<BoxCollider>();
        ground.size = new Vector3(30f, 0.1f, 30f);
        preview = new GameObject("Construction grid preview");
        manager = constructionRoot.AddComponent<BuildingManager>();
        manager.Configure(constructionRoot, preview, validMaterial, invalidMaterial);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || manager == null) return;
        if (keyboard.eKey.wasPressedThisFrame) ToggleMenu();
        if (!menuOpen) return;
        if (keyboard.qKey.wasPressedThisFrame) SelectNextBuilding();
        if (keyboard.rKey.wasPressedThisFrame) manager.RotatePreview();
    }

    public void ToggleMenu()
    {
        menuOpen = !menuOpen;
        if (menuOpen) CityFireEvent.ClosePanel();
        else if (manager != null) manager.ResetDefaultBuilding();
    }

    public void SelectNextBuilding()
    {
        if (manager == null || cards == null || cards.Length == 0) return;
        selectedIndex = (selectedIndex + 1) % cards.Length;
        if (cards[selectedIndex] != null) manager.ChangeCurrentBuildingCard(cards[selectedIndex]);
        FindAnyObjectByType<TownCameraController>()?.FocusOnGround(constructionRoot.transform.position);
    }

    private void OnGUI()
    {
        if (manager == null || CityFireEvent.IsMenuOpen) return;
        if (!menuOpen)
        {
            if (GUI.Button(Tab, "Construction (E)")) ToggleMenu();
            return;
        }
        GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = 18 };
        GUILayout.BeginArea(Panel, GUI.skin.box);
        GUILayout.Label("CONSTRUCTION (E: close, Q: select, R: rotate)", title);
        GUILayout.Label(manager.Status);
        foreach (BuildingCard card in cards)
        {
            if (card == null) continue;
            string label = card.name == "GazStation" ? "Business" : card.name;
            if (GUILayout.Button(label, button, GUILayout.Height(28)))
            {
                FindAnyObjectByType<TownCameraController>()?.FocusOnGround(constructionRoot.transform.position);
                manager.ChangeCurrentBuildingCard(card);
            }
        }
        if (manager.HasSelection && GUILayout.Button("Place building", button, GUILayout.Height(34)))
            manager.PlaceSelectedInBuildArea();

        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        if (constructionRoot != null) Destroy(constructionRoot);
        if (preview != null) Destroy(preview);
    }
}
