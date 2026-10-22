using UnityEngine;
using UnityEngine.AI;

public class StaticResource : MonoBehaviour
{
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
        if (!IsActiveResource)
            return;

        IsActiveResource = false;
        IsAvailable = false;
        DisableComponents();
    }

    public void Configure(ResourceType type)
    {
        ResourceManager.UnregisterResource(resourceType, transform);

        resourceType = type;

        ResourceManager.RegisterResource(resourceType, transform);
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
