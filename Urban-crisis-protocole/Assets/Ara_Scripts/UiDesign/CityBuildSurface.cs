using System.Collections.Generic;
using UnityEngine;

// The buildable surface is the actual map, not an invisible flat construction platform.
public class CityBuildSurface : MonoBehaviour
{
    [SerializeField, Range(0f, 45f)] private float maximumSlope = 20f;
    [SerializeField] private float maximumHeightDifference = 0.5f;
    private Collider ground;
    private Renderer[] scenery;
    public Bounds Bounds => ground != null ? ground.bounds : new Bounds();

    public bool Initialize()
    {
        var terrain = Terrain.activeTerrain;
        GameObject groundObject = terrain != null ? terrain.gameObject : GameObject.Find("Ground_Grass_Base");
        if (groundObject == null) return false;
        ground = groundObject.GetComponent<Collider>();
        if (ground == null && groundObject.GetComponent<MeshFilter>() != null)
            ground = groundObject.AddComponent<BoxCollider>();
        if (ground == null) return false;
        RefreshScenery();
        return true;
    }

    public void RefreshScenery() => scenery = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
    public bool Raycast(Ray ray, out RaycastHit hit)
    {
        hit = default;
        return ground != null && ground.Raycast(ray, out hit, 2000f);
    }

    private bool Sample(float x, float z, out RaycastHit hit)
    {
        Bounds bounds = ground.bounds;
        return ground.Raycast(new Ray(new Vector3(x, bounds.max.y + 10f, z), Vector3.down), out hit, bounds.size.y + 20f);
    }

    private static bool NamedObstacle(Transform t)
    {
        for (; t != null; t = t.parent)
        {
            string n = t.name;
            // The authored map uses decorative meshes without colliders for these features.
            if (n.IndexOf("mountain", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.StartsWith("Hill_", System.StringComparison.OrdinalIgnoreCase) ||
                n.IndexOf("ruin", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("rubble", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.StartsWith("Bridge_", System.StringComparison.OrdinalIgnoreCase) ||
                n.StartsWith("Broken_Bridge", System.StringComparison.OrdinalIgnoreCase) ||
                n.StartsWith("River_Water", System.StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    public bool Validate(Vector3[] cells, float cellSize, out float height, out string reason)
    {
        height = 0f; reason = "Terrain non constructible.";
        if (ground == null || cells == null || cells.Length == 0) return false;
        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        float minHeight = float.MaxValue, maxHeight = float.MinValue;
        float half = cellSize * 0.5f;
        foreach (Vector3 cell in cells)
        {
            minX = Mathf.Min(minX, cell.x - half); maxX = Mathf.Max(maxX, cell.x + half);
            minZ = Mathf.Min(minZ, cell.z - half); maxZ = Mathf.Max(maxZ, cell.z + half);
            // Sample cell corners as well as centres so slopes and map edges cannot be bypassed.
            for (int i = 0; i < 5; i++)
            {
                float x = cell.x + (i == 0 ? 0 : (i <= 2 ? -half + .001f : half - .001f));
                float z = cell.z + (i == 0 ? 0 : (i % 2 == 1 ? -half + .001f : half - .001f));
                if (!Sample(x, z, out RaycastHit hit) || Vector3.Angle(hit.normal, Vector3.up) > maximumSlope) return false;
                minHeight = Mathf.Min(minHeight, hit.point.y); maxHeight = Mathf.Max(maxHeight, hit.point.y);
            }
        }
        if (maxHeight - minHeight > maximumHeightDifference) return false;
        height = maxHeight;
        Bounds footprint = new Bounds(new Vector3((minX + maxX) * .5f, height + 1f, (minZ + maxZ) * .5f), new Vector3(maxX - minX - .002f, 2f, maxZ - minZ - .002f));
        foreach (Collider obstacle in Physics.OverlapBox(footprint.center, footprint.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
        {
            if (obstacle == ground || obstacle.GetComponentInParent<CityBuildSurface>() != null) continue;
            reason = "Obstacle ou bâtiment existant."; return false;
        }
        if (scenery == null) RefreshScenery();
        foreach (Renderer item in scenery)
        {
            if (item == null || !item.enabled || !item.gameObject.activeInHierarchy || item.gameObject == ground.gameObject) continue;
            StaticResource resource = item.GetComponentInParent<StaticResource>();
            if (!(NamedObstacle(item.transform) || (resource != null && resource.IsActiveResource) || item.GetComponentInParent<CityStructure>() != null)) continue;
            Bounds bounds = item.bounds;
            if (bounds.max.x > minX && bounds.min.x < maxX && bounds.max.z > minZ && bounds.min.z < maxZ && bounds.max.y > height - .1f)
            { reason = "Obstacle, montagne ou surface non constructible."; return false; }
        }
        reason = null; return true;
    }
}
