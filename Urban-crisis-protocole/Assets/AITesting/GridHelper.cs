using UnityEngine;

public class GridHelper : MonoBehaviour
{
    private Renderer mesh;
    private Grid grid;

    void Start()
    {
        mesh = GetComponent<Renderer>();
        grid = GetComponent<Grid>();
    }

    public Vector3 GetRandomCell()
    {
        Bounds limit = mesh.bounds;

        float x = Random.Range(limit.min.x, limit.max.x);
        float y = Random.Range(limit.min.y, limit.max.y);
        float z = Random.Range(limit.min.z, limit.max.z);
        Vector3 worldPos = new(x, y, z);

        Vector3 cellCenterPos = GetCellCenter(worldPos);

        return cellCenterPos;
    }

    public Vector3 GetCellCenter(Vector3 position)
    {
       Vector3Int cell = grid.WorldToCell(position);
        Vector3 caseCenter = grid.GetCellCenterWorld(cell);

        return caseCenter;
    }
}
