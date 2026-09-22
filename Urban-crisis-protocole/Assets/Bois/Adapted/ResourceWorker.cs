using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class ResourceWorker : Worker
{
    PlayerInfo playerInfo = PlayerInfo.Instance;

    private Transform[] _resourceTransforms;
    private Transform currentResource;
    private ResourceType cachedResourceType;

    private Vector3 _stationPosition;

    private NavMeshAgent agent;
    private event Action OnResourceFound;
    private event Action OnResourceBroughtBack; // Reach work station
    private event Action OnResourceCollected; // Harvested resource

    private readonly float collectionTime = 2f;
    private bool goingToResource = true;

    private bool IsNearOldDestination = false;

    private bool isRetrying = false;

    private bool isWorking = false; // Has started moving towards first tree

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = UnityEngine.Random.Range(5, 80);

        OnResourceBroughtBack += HandleResourceBroughtBack;
        OnResourceCollected += HandleResourceCollected;
        OnResourceFound += GoToNextResource;
    }

    public void InitializeResourceWorker(Transform[] resourceTransforms, Vector3 stationPosition)
    {
        _resourceTransforms = resourceTransforms;
        _stationPosition = stationPosition;
        FindNextResource();
    }

    void Update()
    {
        VerifyDestination();
    }

    private void VerifyDestination()
    {
        if (isWorking)
        {
            if (isRetrying) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // Reach destination
            {
                if (IsNearOldDestination) return; // Prevent the conditions from firing again while the agent is leaving previous destination
                IsNearOldDestination = true;
                if (goingToResource)
                {
                    // Reached tree, Start collecting wood
                    goingToResource = false;
                    StartCoroutine(CollectResourceCoroutine());
                }
                else
                {
                    // Reached Hut, Go back to tree
                    goingToResource = true;
                    OnResourceBroughtBack?.Invoke();
                    FindNextResource();
                }
            }
            else
            {
                IsNearOldDestination = false;
            }
        }
    }

    private IEnumerator CollectResourceCoroutine()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(collectionTime);
        agent.isStopped = false;
        OnResourceCollected?.Invoke();
        agent.SetDestination(_stationPosition);
    }

    private void HandleResourceCollected()
    {
        if (currentResource == null) return;
        cachedResourceType = GetCurrentResourceScript().ResourceType;
        GetCurrentResourceScript().Disable();
        GetCurrentResourceScript().IsAvailable = true;
        currentResource = null;
    }

    private void HandleResourceBroughtBack()
    {
        playerInfo.addResource(cachedResourceType, 1);
    }

    private void GoToNextResource()
    {
        // Go to that resource
        agent.SetDestination(currentResource.position);

        isWorking = true;

        GetCurrentResourceScript().IsAvailable = false;
    }

    // Find an existing resource that is not taken by another npc
    private void FindNextResource()
    {
        if (!AttemptFindNextResource())
        {
            if (!isRetrying) // Prevent more than one retry coroutines at once
            {
                isRetrying = true;
                StartCoroutine(RetryDelayCoroutine());
            }
            else
            {
                agent.SetDestination(_stationPosition);
            }
        }
        else
        {
            OnResourceFound?.Invoke();
        }
    }

    private bool AttemptFindNextResource()
    {
        bool success;

        int rng = UnityEngine.Random.Range(0, _resourceTransforms.Length);
        currentResource = _resourceTransforms[rng];

        if (GetCurrentResourceScript().IsAvailable && GetCurrentResourceScript().IsActiveResource)
        {
            success = true;
        }
        else
        {
            success = false;
        }
        return success;
    }

    private IEnumerator RetryDelayCoroutine()
    {
        yield return new WaitForSeconds(2.0f);

        isRetrying = false;
        FindNextResource();
    }

    private StaticResource GetCurrentResourceScript()
    {
        if (currentResource == null)
            return null;

        return currentResource.gameObject.GetComponent<StaticResource>();
    }

    public override void Delete()
    {
        var resourceScript = GetCurrentResourceScript();

        if (resourceScript != null)
        {
            resourceScript.IsAvailable = true;
        }

        base.Delete();
    }

    void OnDestroy()
    {
        OnResourceBroughtBack -= HandleResourceBroughtBack;
        OnResourceCollected -= HandleResourceCollected;
        OnResourceFound -= GoToNextResource;
    }
}
