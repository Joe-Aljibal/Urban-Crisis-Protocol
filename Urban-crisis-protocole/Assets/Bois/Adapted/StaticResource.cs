using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StaticResource : MonoBehaviour
{
    [SerializeField] private float respawnTime = 2.0f;
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private bool staysDepleted;
    
    public ResourceType ResourceType => resourceType;
    public bool IsActiveResource { get; set; } = true;
    public bool IsAvailable { get; set; } = true;

    void Awake()
    {
        ResourceManager.RegisterResource(resourceType, transform);
    }

    public void Disable()
    {
        if (!IsActiveResource)
            return;

        IsActiveResource = false;
        IsAvailable = false;
        DisableComponents();

        if (!staysDepleted)
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    public void Configure(
        ResourceType type,
        bool remainDepleted,
        float regenerationTime
    )
    {
        ResourceManager.UnregisterResource(resourceType, transform);

        resourceType = type;
        staysDepleted = remainDepleted;
        respawnTime = regenerationTime;

        ResourceManager.RegisterResource(resourceType, transform);
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        ActivateComponents();
    }

    private void ActivateComponents()
    {
        SetComponentsEnabled(true);
        IsActiveResource = true;
        IsAvailable = true;
    }

    private void DisableComponents()
    {
        SetComponentsEnabled(false);
    }

    private void SetComponentsEnabled(bool value)
    {
        foreach (Renderer resourceRenderer in GetComponentsInChildren<Renderer>())
        {
            resourceRenderer.enabled = value;
        }

        foreach (Collider resourceCollider in GetComponentsInChildren<Collider>())
        {
            resourceCollider.enabled = value;
        }

        foreach (NavMeshObstacle resourceObstacle in GetComponentsInChildren<NavMeshObstacle>())
        {
            resourceObstacle.enabled = value;
        }
    }

    private void OnDestroy()
    {
        ResourceManager.UnregisterResource(resourceType, transform);
    }
}
