using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HutManager : MonoBehaviour
{
    [SerializeField] GameObject woodNpcPrefab;

    [SerializeField] GameObject hutPrefab;

    private List<GameObject> hutStations = new List<GameObject>();

    [SerializeField] Transform hutTransform;
    [SerializeField] Transform[] treeTransforms;
    [SerializeField] WorkStationUIManager workStationUIManager;
    void Awake()
    {
         workStationUIManager.OnAddHutStation += HandleHutCreated;
    }

    private void HandleHutCreated(Vector3 position, int huts, int npcCount)
    {
        for (int i = 0; i < huts; i++)
        {
            int offset = i * 3;
             Vector3 offsetPosition = new Vector3(
                position.x + offset,
                position.y,
                position.z);

        GameObject hut = Instantiate(hutPrefab, offsetPosition, Quaternion.identity);
        HutStation hutScript = hut.GetComponent<HutStation>();

        Vector3[] treePositions = new Vector3[treeTransforms.Length];
            for(int a = 0; a < treeTransforms.Length; a++)
            {
                treePositions[a] = treeTransforms[a].position;
            }
        hutScript.SetTreePositions(treePositions);

        hutScript.InitializeBuilding(offsetPosition, npcCount, woodNpcPrefab);

        hutStations.Add(hut);
        }
    }

    private void HandleWoodNpcAssigned(int number, Vector3 position)
    {
        GameObject npc = Instantiate(woodNpcPrefab, hutTransform.position, Quaternion.identity);
        NpcScript npcScript = npc.GetComponent<NpcScript>();

        Vector3[] treePositions = new Vector3[treeTransforms.Length];
            for(int i = 0; i < treeTransforms.Length; i++)
            {
                treePositions[i] = treeTransforms[i].position;
            }
        npcScript.initializeWoodNpc(treePositions, hutTransform.position);
    }
}
