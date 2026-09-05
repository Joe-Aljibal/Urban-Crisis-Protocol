using UnityEngine;

public class HutStation : WorkStation
{
    protected override GameObject NpcPrefab { 
     get => base.NpcPrefab;
     set => base.NpcPrefab = value; }

    private Transform[] _treeTransforms;
    void Awake()
    {
        buildingCapacity = 5;
    }

    // Override to use wood npc prefab
    public override void InitializeBuilding(Vector3 position, int workerCount, GameObject prefab)
    {
        NpcPrefab = prefab;
        base.InitializeBuilding(position, workerCount, prefab);
    }

    public void SetTreePositions(Transform[] treeTransforms)
    {
        _treeTransforms = treeTransforms;
    }

    // Override to initialize the wood npcs
    protected override Worker CreateSingleNpc(Vector3 position)
    {
        NpcScript npcScript = Instantiate(NpcPrefab, position, Quaternion.identity)
            .GetComponent<NpcScript>();

        npcScript.InitializeWoodNpc(_treeTransforms, transform.position);

        return npcScript;
    }
}
