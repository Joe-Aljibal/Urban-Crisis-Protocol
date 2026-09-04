using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcScript : Worker
{
    private Vector3[] _treePositions;
    private Vector3 _hutPosition;

    private NavMeshAgent agent;
    private event Action OnWoodCollected;
    private float collectionTime = 2f;
    private bool goingToTree = true;

    private bool IsNearOldDestination = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        OnWoodCollected += HandleWoodCollected;
    }

    void Update()
    {

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // Reach destination
        {
            if (IsNearOldDestination) return; // Prevent the conditions from firing again while the agent is leaving previous destination
            IsNearOldDestination = true;
            if (goingToTree)
            {
                // Reached tree, Start collecting wood
                goingToTree = false;
                StartCoroutine(ChopWoodCoroutine());
            }
            else
            {
                // Reached Hut, Go back to tree
                goingToTree = true;
                OnWoodCollected?.Invoke();
                GoToNextTree();
            }
        }
        else
        {
            IsNearOldDestination = false;
        }
    }

    public void initializeWoodNpc(Vector3[] treePositions, Vector3 hutPosition)
    {
        _treePositions = treePositions;
        _hutPosition = hutPosition;
        GoToNextTree();
    }
    private IEnumerator ChopWoodCoroutine()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(collectionTime);
        agent.isStopped = false;
        agent.SetDestination(_hutPosition);
    }

    private void HandleWoodCollected()
    {
        // Add ressource
    }

    private void GoToNextTree()
    {
        int rng = UnityEngine.Random.Range(0, _treePositions.Length);
        agent.SetDestination(_treePositions[rng]);
    }

    void OnDestroy()
    {
        OnWoodCollected -= HandleWoodCollected;
    }
}
