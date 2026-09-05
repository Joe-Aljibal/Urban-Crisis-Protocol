using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))] // Automatically add NavMeshAgent component
public class NpcScript : Worker
{
    private Transform[] _treeTransforms;

    private Transform currentTree;

    private Vector3 _hutPosition;

    private NavMeshAgent agent;
    private event Action OnTreeFound;
    private event Action OnWoodCollected;
    private event Action OnTreeChopped;

    [SerializeField] private float collectionTime = 2f;
    private bool goingToTree = true;

    private bool IsNearOldDestination = false;

    private bool isWorking = false; // Has started moving towards first tree

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        OnWoodCollected += HandleWoodCollected;
        OnTreeChopped += HandleTreeChopped;
        OnTreeFound += GoToNextTree;
    }
    
    public void InitializeWoodNpc(Transform[] treePositions, Vector3 hutPosition)
    {
        _treeTransforms = treePositions;
        _hutPosition = hutPosition;
        FindNextTree();
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
                    FindNextTree();
                }
            }
            else
            {
                IsNearOldDestination = false;
            }
        }
    }

    private IEnumerator ChopWoodCoroutine()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(collectionTime);
        agent.isStopped = false;
        OnTreeChopped?.Invoke();
        agent.SetDestination(_hutPosition);
    }

    private void HandleTreeChopped()
    {
        GetCurrentTreeScript().DisableTree();
        GetCurrentTreeScript().IsAvailable = true;
        currentTree = null;
    }

    private void HandleWoodCollected()
    {
        // Add ressource
    }

    private void GoToNextTree()
    {
        // Go to that tree
        agent.SetDestination(currentTree.position);

        isWorking = true;

        GetCurrentTreeScript().IsAvailable = false;
    }

    // Find an existing tree that is not taken by another npc
    private void FindNextTree()
    {
        if (!AttemptFindNextTree())
        {
            StartCoroutine(RetryDelayCoroutine());
        } else
        {
            OnTreeFound?.Invoke();
        }
    }

    private bool AttemptFindNextTree()
    {
        bool success;

        int rng = UnityEngine.Random.Range(0, _treeTransforms.Length);
        currentTree = _treeTransforms[rng];
        
            if (GetCurrentTreeScript().IsAvailable && GetCurrentTreeScript().IsActiveRessource)
            {
                success = true;
            }
            else
            {
                success = false;
                currentTree = null;
            }
        return success;
    }

    private IEnumerator RetryDelayCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        FindNextTree();
    }

    private TreeScript GetCurrentTreeScript()
    {
        return currentTree.gameObject.GetComponent<TreeScript>();
    }

    public override void Delete()
    {
        if (GetCurrentTreeScript() != null)
        {
            GetCurrentTreeScript().IsAvailable = true;
        }
        base.Delete();
    }

    void OnDestroy()
    {
        OnWoodCollected -= HandleWoodCollected;
        OnTreeChopped -= HandleTreeChopped;
        OnTreeFound -= GoToNextTree;
    }
}
