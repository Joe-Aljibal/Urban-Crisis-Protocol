using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HutManager : MonoBehaviour
{
    /* This class manages all huts in the scene */

    [SerializeField] GameObject woodNpcPrefab;

    [SerializeField] GameObject hutPrefab;

    private readonly List<GameObject> hutStations = new();

    [SerializeField] Transform hutTransform;
    [SerializeField] Transform[] treeTransforms;
    [SerializeField] WorkStationUIManager workStationUIManager;

    void Awake()
    {
        workStationUIManager.OnAddHutStation += HandleHutCreated;
        workStationUIManager.OnAddNpc += HandleWoodNpcAssigned;
        workStationUIManager.OnDeleteHutStation += HandleHutDeleted;
        workStationUIManager.OnDeleteWoodNpc += HandleWoodNpcDeleted;
    }

    private void HandleHutCreated(Vector3 position, int huts, int npcCount)
    {
        for (int i = 0; i < huts; i++)
        {
            int offset = i * 3;
            Vector3 offsetPosition = new(
               position.x + offset,
               position.y,
               position.z);

            GameObject hut = Instantiate(hutPrefab, offsetPosition, Quaternion.identity);
            HutStation hutScript = hut.GetComponent<HutStation>();

            Vector3[] treePositions = new Vector3[treeTransforms.Length];
            for (int a = 0; a < treeTransforms.Length; a++)
            {
                treePositions[a] = treeTransforms[a].position;
            }
            hutScript.SetTreePositions(treeTransforms);

            hutScript.InitializeBuilding(offsetPosition, npcCount, woodNpcPrefab);

            hutStations.Add(hut);
        }
    }

    private void HandleWoodNpcAssigned(Vector3 position, int number)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.AssignNpcs(number);
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

    private void HandleWoodNpcDeleted(Vector3 position, int number)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach(var col in hits)
        {
            
            if(col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.RemoveNpcs(number);
            }
        }
    }

    void OnDestroy()
    {
        workStationUIManager.OnAddHutStation -= HandleHutCreated;
        workStationUIManager.OnAddNpc -= HandleWoodNpcAssigned;
        workStationUIManager.OnDeleteHutStation -= HandleHutDeleted;
        workStationUIManager.OnDeleteWoodNpc -= HandleWoodNpcDeleted;
    }


}
