using UnityEngine;

public class HutStation : WorkStation
{
    protected override GameObject NpcPrefab { 
     get => base.NpcPrefab;
     set => base.NpcPrefab = value; }

    private Transform[] _resourceTransforms;
    
    // Override to use resource npc prefab
    public override void InitializeBuilding(Vector3 position, int workerCount, GameObject prefab)
    {
        NpcPrefab = prefab;
        base.InitializeBuilding(position, workerCount, prefab);
    }

    public void SetResourceTransforms(Transform[] resourceTransforms)
    {
        _resourceTransforms = resourceTransforms;
    }

    // Override to initialize the resource npcs
    protected override Worker CreateSingleWorker(Vector3 position)
    {
        ResourceWorker resourceWorker = Instantiate(NpcPrefab, position, Quaternion.identity)
            .GetComponent<ResourceWorker>();

        resourceWorker.InitializeResourceWorker(_resourceTransforms, transform.position);

        return resourceWorker;
    }
}
