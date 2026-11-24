using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Building/PlacementRules")]
public class BuildingPlacementRules : ScriptableObject
{
    public Vector3[] CanPlaceBuilding(Vector3 futurPosition, List<Vector3> filledSlot, int width, int height, Grid grid)
    {

        float cellSize = 0.5f;

        const float eps = 0.001f;
        Bounds groundBounds = grid != null ? GetGroundBounds(grid) : new Bounds(Vector3.zero, Vector3.one * 100000f);

        List<Vector3> occupiedCells = new();

        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // verifier si les position 
                Vector3 pos =
                    futurPosition +
                    new Vector3(
                        x * cellSize,
                        0,
                        z * cellSize);

                // hors du sol
                if (pos.x < groundBounds.min.x - eps || pos.x > groundBounds.max.x + eps ||
                    pos.z < groundBounds.min.z - eps || pos.z > groundBounds.max.z + eps)
                    return null;

                if (FindPositionInList(pos, filledSlot))
                {
                    return null;
                }

                occupiedCells.Add(pos);
            }
        }

        return occupiedCells.ToArray();
    }

    public Vector3 ChangeFuturPosition(Vector3 futurPosition, int width, int height)
    {
        float cellSize = 0.5f;
        return futurPosition +
           new Vector3(
               (width - 1) * cellSize / 2f,
               0,
               (height - 1) * cellSize / 2f
           );
    }

    Bounds GetGroundBounds(Grid grid)
    {
        Collider col = grid.GetComponentInChildren<Collider>();
        return col != null ? col.bounds : grid.GetComponentInChildren<Renderer>().bounds;
    }

    public bool FindPositionInList(Vector3 position, List<Vector3> filledSlot)
    {
        return filledSlot.Contains(position);
    }
    public virtual Vector3[] CanPlaceBuilding(Vector3 position, List<Vector3> occupied)
        => CanPlaceBuilding(position, occupied, 1, 1, null);
    public virtual Vector3 ChangeFuturPosition(Vector3 position) => position;

}
