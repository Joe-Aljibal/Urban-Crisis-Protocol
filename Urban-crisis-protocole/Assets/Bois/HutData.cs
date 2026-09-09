using UnityEngine;

[CreateAssetMenu(fileName = "HutType", menuName = "Buildings/HutData")]
public class HutData : ScriptableObject
{
    public HutType hutType;
    public ResourceType resourceType;
    public GameObject hutPrefab;
    public GameObject WorkerPrefab;
}
