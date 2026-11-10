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
    private GameObject preview;
    private static CityConstruction instance;
    private static Rect Panel => new Rect(430f, 20f, 210f, 160f);
    public static bool IsPointerOverPanel => instance != null && Mouse.current != null &&
        Panel.Contains(new Vector2(Mouse.current.position.ReadValue().x, Screen.height - Mouse.current.position.ReadValue().y));

    private void Start()
    {
        instance = this;
        constructionRoot = new GameObject("Construction area");
        constructionRoot.transform.position = new Vector3(-120f, 0.15f, -55f);
        Grid grid = constructionRoot.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f, 0.5f, 0.5f);
        grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        BoxCollider ground = constructionRoot.AddComponent<BoxCollider>();
        ground.size = new Vector3(30f, 0.1f, 30f);
        preview = new GameObject("Construction grid preview");
        manager = constructionRoot.AddComponent<BuildingManager>();
        manager.Configure(constructionRoot, preview, validMaterial, invalidMaterial);
    }

    private void OnGUI()
    {
        if (manager == null) return;
        GUILayout.BeginArea(Panel, GUI.skin.box);
        GUILayout.Label("CONSTRUCTION");
        foreach (BuildingCard card in cards)
        {
            if (card == null) continue;
            if (GUILayout.Button(card.name)) manager.ChangeCurrentBuildingCard(card);
        }
        if (GUILayout.Button("Focus construction area"))
            FindAnyObjectByType<TownCameraController>()?.FocusOnGround(constructionRoot.transform.position);
        if (GUILayout.Button("Cancel placement")) manager.ResetDefaultBuilding();
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        if (constructionRoot != null) Destroy(constructionRoot);
        if (preview != null) Destroy(preview);
    }
}
