using System.Collections.Generic;
using UnityEngine;

public class SimpleResourceCollector : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float exploitationTime = 1.5f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private int resourceAmount = 1;

    private Vector3 homePosition;
    private StaticResource targetResource;
    private ResourceType carriedResourceType;
    private ResourceType nextResourceType = ResourceType.Wood;
    private List<StaticResource> exploitedThisRound = new List<StaticResource>();
    private float exploitationTimer;
    private float searchTimer;
    private bool isExploiting;
    private bool isReturningHome;

    public string CurrentAction { get; private set; } = "Recherche d'une ressource";
    public int DeliveredResources { get; private set; }

    private void Start()
    {
        homePosition = transform.position;
    }

    private void Update()
    {
        if (isReturningHome)
        {
            ReturnHome();
            return;
        }

        if (targetResource == null)
        {
            isExploiting = false;
            SearchForResource();
            return;
        }

        if (isExploiting)
        {
            ExploitResource();
            return;
        }

        MoveToResource();
    }

    private void SearchForResource()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer > 0f)
            return;

        searchTimer = 1f;
        targetResource = FindClosestResource();

        if (targetResource != null)
        {
            targetResource.IsAvailable = false;
            nextResourceType = GetOtherResourceType(targetResource.ResourceType);
            CurrentAction = "Déplacement vers " + GetResourceName(targetResource.ResourceType);
        }
        else
        {
            exploitedThisRound.Clear();
            CurrentAction = "Recherche d'une ressource";
        }
    }

    private StaticResource FindClosestResource()
    {
        StaticResource resource = FindClosestResource(nextResourceType);

        if (resource == null)
        {
            resource = FindClosestResource(GetOtherResourceType(nextResourceType));
        }

        return resource;
    }

    private StaticResource FindClosestResource(ResourceType resourceType)
    {
        Transform[] resources = ResourceManager.GetResourceTransforms(resourceType);

        StaticResource closestResource = null;
        float closestDistance = float.MaxValue;

        foreach (Transform resourceTransform in resources)
        {
            StaticResource resource = resourceTransform.GetComponent<StaticResource>();

            if (!resource.IsAvailable ||
                !resource.IsActiveResource ||
                exploitedThisRound.Contains(resource))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, resourceTransform.position);

            if (distance < closestDistance)
            {
                closestResource = resource;
                closestDistance = distance;
            }
        }

        return closestResource;
    }

    private void MoveToResource()
    {
        if (MoveTo(targetResource.transform.position))
        {
            isExploiting = true;
            exploitationTimer = exploitationTime;
            CurrentAction = "Exploitation du " + GetResourceName(targetResource.ResourceType);
        }
    }

    private void ExploitResource()
    {
        exploitationTimer -= Time.deltaTime;

        if (exploitationTimer > 0f)
            return;

        carriedResourceType = targetResource.ResourceType;
        exploitedThisRound.Add(targetResource);
        targetResource.Disable();
        targetResource = null;
        isExploiting = false;
        isReturningHome = true;
        CurrentAction = "Retour avec du " + GetResourceName(carriedResourceType);
    }

    private void ReturnHome()
    {
        if (!MoveTo(homePosition))
            return;

        PlayerInfo.Instance.addResource(carriedResourceType, resourceAmount);
        DeliveredResources += resourceAmount;
        isReturningHome = false;
        searchTimer = 0.5f;
        CurrentAction = "Ressource déposée";
    }

    private ResourceType GetOtherResourceType(ResourceType resourceType)
    {
        if (resourceType == ResourceType.Wood)
            return ResourceType.Stone;

        return ResourceType.Wood;
    }

    private string GetResourceName(ResourceType resourceType)
    {
        if (resourceType == ResourceType.Wood)
            return "bois";

        return "pierre";
    }

    private bool MoveTo(Vector3 destination)
    {
        destination.y = transform.position.y;

        Vector3 direction = destination - transform.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        return Vector3.Distance(transform.position, destination) <= stoppingDistance;
    }

    private void OnDisable()
    {
        if (targetResource != null && targetResource.IsActiveResource)
        {
            targetResource.IsAvailable = true;
        }
    }
}
