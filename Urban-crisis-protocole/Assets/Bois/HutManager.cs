using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class manages all huts in the scene
/// </summary>

public class HutManager : MonoBehaviour
{
    [SerializeField] GameObject woodHutPrefab;
    [SerializeField] GameObject woodNpcPrefab;

    private readonly List<GameObject> hutStations = new();

    [SerializeField] Transform hutTransform;
    [SerializeField] Transform[] treeTransforms;
    [SerializeField] WorkStationUIManager workStationUIManager;

    void Awake()
    {
        workStationUIManager.OnAddHutStation += HandleHutCreated;
        workStationUIManager.OnAddWorker += HandleWorkerAssigned;
        workStationUIManager.OnDeleteHutStation += HandleHutDeleted;
        workStationUIManager.OnDeleteWorker += HandleWorkerDeleted;
    }

    private void HandleHutCreated(Vector3 position, HutType hutType, int numberHuts, int numberWorkers)
    {
            switch (hutType)
            {
                case HutType.WoodHut:
                CreateHut(position, numberHuts, numberWorkers, treeTransforms, woodHutPrefab, woodNpcPrefab);
                    break;
            }
    }

    private void HandleWorkerAssigned(Vector3 position, int number)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.AssignWorkers(number);
            }
        }
    }

    private void HandleHutDeleted(Vector3 position)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach(var col in hits)
        {
            if(col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.DestroyBuilding();
            }
        }
    }

    private void HandleWorkerDeleted(Vector3 position, int number)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach(var col in hits)
        {
            
            if(col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.RemoveWorkers(number);
            }
        }
    }

    private void CreateHut(Vector3 position, int numberHuts, int numberWorkers,
     Transform[] resourceTransforms, GameObject hutPrefab, GameObject workerPrefab)
    {
        for (int i = 0; i < numberHuts; i++)
        {
            int offset = i * 3;
            Vector3 offsetPosition = new(
               position.x + offset,
               position.y,
               position.z);

            GameObject hut = Instantiate(hutPrefab, offsetPosition, Quaternion.identity);
            HutStation hutScript = hut.GetComponent<HutStation>();

            hutScript.SetResourceTransforms(resourceTransforms);

            hutScript.InitializeBuilding(offsetPosition, numberWorkers, workerPrefab);

            hutStations.Add(hut);
        }
    }

    void OnDestroy()
    {
        workStationUIManager.OnAddHutStation -= HandleHutCreated;
        workStationUIManager.OnAddWorker -= HandleWorkerAssigned;
        workStationUIManager.OnDeleteHutStation -= HandleHutDeleted;
        workStationUIManager.OnDeleteWorker -= HandleWorkerDeleted;
    }


}
