using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private Transform[] treeTransforms;
    public Transform[] TreeTransforms => treeTransforms;
}
