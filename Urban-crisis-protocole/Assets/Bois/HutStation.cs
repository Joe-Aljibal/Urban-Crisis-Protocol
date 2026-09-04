using UnityEngine;

public class HutStation : WorkStation
{
    private GameObject woodNpcPrefab;
    private Vector3[] _treePositions;
    void Awake()
    {
        buildingCapacity = 5;
    }

    public override void InitializeBuilding(Vector3 position, int workerCount, GameObject prefab)
    {
        woodNpcPrefab = prefab;
        base.InitializeBuilding(position, workerCount, prefab);
    }

    public void SetTreePositions(Vector3[] treePositions)
    {
        _treePositions = treePositions;
    }

    public override void AssignNpcs(int number)
    {
        base.AssignNpcs(number);
        CreateNpcs(number);
    }
    public override void CreateNpcs(int number)
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 offsetPosition = new Vector3(
                transform.position.x + i,
                transform.position.y,
                transform.position.z);
            NpcScript woodNpcScript = Instantiate(woodNpcPrefab, offsetPosition, Quaternion.identity)
            .GetComponent<NpcScript>();
            woodNpcScript.initializeWoodNpc(_treePositions, transform.position);
            WorkerList.Add(woodNpcScript);
        }
    }
}
