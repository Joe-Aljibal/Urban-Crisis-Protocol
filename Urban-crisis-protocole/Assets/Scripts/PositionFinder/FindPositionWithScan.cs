using UnityEngine;

public class FindPositionWithScan : IFindPosition
{
    private  Grid grid;
    private  Vector3Int min, max;
    private int cursorX, cursorZ;

    public int CellCount => (max.x - min.x + 1) * (max.z - min.z + 1);

    public FindPositionWithScan(GameObject ground)
    {
        grid = ground.GetComponent<Grid>();

        Collider col = ground.GetComponentInChildren<Collider>();
        Bounds b = col != null ? col.bounds : ground.GetComponentInChildren<Renderer>().bounds;

        min = grid.WorldToCell(b.min + Vector3.one * 0.01f);
        max = grid.WorldToCell(b.max - Vector3.one * 0.01f);
        Reset();
    }

    public void Reset() { cursorX = min.x; cursorZ = min.z; }

    public Vector3? FindPosition()
    {
        Vector3 center = grid.GetCellCenterWorld(new Vector3Int(cursorX, 0, cursorZ));
        center.y = grid.transform.position.y + 0.01f;
        Advance();
        return center;
    }

    void Advance()
    {
        if (++cursorX > max.x)
        {
            cursorX = min.x;
            if (++cursorZ > max.z) cursorZ = min.z;
        }
    }
}
