using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StaticResource : MonoBehaviour
{
    [SerializeField] private float respawnTime = 2.0f;
    [SerializeField] private ResourceType resourceType;
    
    public ResourceType ResourceType => resourceType;
    public bool IsActiveResource { get; set; } = true;
    public bool IsAvailable { get; set; } = true;

    void Awake()
    {
        ResourceManager.RegisterResource(resourceType, transform);
    }

    public void Disable()
    {
        IsActiveResource = false;
        DisableComponents();
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        ActivateComponents();
    }

    private void ActivateComponents()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
        IsActiveResource = true;
    }

    private void DisableComponents()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<NavMeshObstacle>().enabled = false;
    }
}
