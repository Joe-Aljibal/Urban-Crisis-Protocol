using UnityEngine;

/// <summary>
/// Manages its own hut's workers
/// </summary>

public class HutManager : WorkStation
{
    public HutData hutData;
    private HutStation hutStation;

    // Override to initialize the resource worker
    protected override Worker CreateSingleWorker(Vector3 position)
    {
        ResourceWorker resourceWorker = Instantiate(hutData.WorkerPrefab, position, Quaternion.identity)
            .GetComponent<ResourceWorker>();

        resourceWorker.InitializeResourceWorker(
            ResourceManager.GetResourceTransforms(hutData.resourceType), transform.position);

        return resourceWorker;
    }
}