using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempWoodManager : MonoBehaviour
{
    [SerializeField] GameObject npcPrefab;

    [SerializeField] Transform hutTransform;
    [SerializeField] Transform[] treeTransforms;
    void Start()
    {  
    }

    void Update()
    {
        if(Keyboard.current.dKey.wasPressedThisFrame)
        {
            CreateWoodNpc();
        }
    }

    private void CreateWoodNpc()
    {
        GameObject npc = Instantiate(npcPrefab, hutTransform.position, Quaternion.identity);
        NpcScript npcScript = npc.GetComponent<NpcScript>();

        Vector3[] treePositions = new Vector3[treeTransforms.Length];
            for(int i = 0; i < treeTransforms.Length; i++)
            {
                treePositions[i] = treeTransforms[i].position;
            }
        npcScript.initializeWoodNpc(treePositions, hutTransform.position);
    }
}
