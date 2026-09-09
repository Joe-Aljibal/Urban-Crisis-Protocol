using UnityEngine;

public class LocalHutManager : MonoBehaviour
{
    [SerializeField] public HutData hutData;
    //private ResourceManager resourceManager { get; set; }
    private HutStationOld hutStationOld;
    private HutStation hutStation;

    void Start()
    {
        hutStation = GetComponent<HutStation>();
        hutStation.InitializeBuilding(new Vector3(0,0,0), 0, hutData.WorkerPrefab);
        hutStation.SetResourceTransforms(ResourceManager.
        GetResourceTransforms(hutData.resourceType));
    }

    void Update()
    {
        
    }
}