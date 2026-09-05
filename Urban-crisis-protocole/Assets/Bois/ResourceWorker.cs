using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class ResourceWorker : Worker
{
    private Transform[] _resourceTransforms;
    private Transform currentRessource;
    
    private Vector3 _stationPosition;

    private NavMeshAgent agent;
    private event Action OnResourceFound;
    private event Action OnResourceBroughtBack; // Reach work station
    private event Action OnResourceCollected; // Harvested ressource

    private float collectionTime = 2f;
    private bool goingToRessource = true;

    private bool IsNearOldDestination = false;

    private bool isWorking = false; // Has started moving towards first tree

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

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
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // Reach destination
            {
                if (IsNearOldDestination) return; // Prevent the conditions from firing again while the agent is leaving previous destination
                IsNearOldDestination = true;
                if (goingToRessource)
                {
                    // Reached tree, Start collecting wood
                    goingToRessource = false;
                    StartCoroutine(CollectResourceCoroutine());
                }
                else
                {
                    // Reached Hut, Go back to tree
                    goingToRessource = true;
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
        GetCurrentResourceScript().Disable();
        GetCurrentResourceScript().IsAvailable = true;
        currentRessource = null;
    }

    private void HandleResourceBroughtBack()
    {
        // Add ressource
    }

    private void GoToNextResource()
    {
        // Go to that resource
        agent.SetDestination(currentRessource.position);

        isWorking = true;

        GetCurrentResourceScript().IsAvailable = false;
    }

    // Find an existing resource that is not taken by another npc
    private void FindNextResource()
    {
        if (!AttemptFindNextResource())
        {
            StartCoroutine(RetryDelayCoroutine());
        } else
        {
            OnResourceFound?.Invoke();
        }
    }

    private bool AttemptFindNextResource()
    {
        bool success;

        int rng = UnityEngine.Random.Range(0, _resourceTransforms.Length);
        currentRessource = _resourceTransforms[rng];
        
            if (GetCurrentResourceScript().IsAvailable && GetCurrentResourceScript().IsActiveRessource)
            {
                success = true;
            }
            else
            {
                success = false;
                currentRessource = null;
            }
        return success;
    }

    private IEnumerator RetryDelayCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        FindNextResource();
    }

    private StaticResource GetCurrentResourceScript()
    {
        if(currentRessource == null) return null;

        return currentRessource.gameObject.GetComponent<StaticResource>();
    }

    public override void Delete()
    {
        var ressourceScript = GetCurrentResourceScript();

        if (ressourceScript != null)
        {
            ressourceScript.IsAvailable = true;
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
