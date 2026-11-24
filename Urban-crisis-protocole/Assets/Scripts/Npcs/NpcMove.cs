using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This class is responsible for controlling individual wondering NPCs.
/// </summary>

[RequireComponent(typeof(NavMeshAgent))]
public class NpcMove : MonoBehaviour
{
    [SerializeField] private float wonderRadius = 15.0f;
    [SerializeField] private float MinWonderSpeed = 0.5f;
    [SerializeField] private float MaxWonderSpeed = 4f;

    private NavMeshAgent agent;
    private float interval = 3.0f;
    private float timer = 0;

    private Coroutine RetryCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = UnityEngine.Random.Range(5, 80);
        
        WanderToNextLocation();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            //if (timer >= interval)
           // {
                WanderToNextLocation();
                timer = 0;
           // }
        }
    }

    // Generate random point within radius to go to
    private void WanderToNextLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wonderRadius;
        randomDirection += transform.position;

    

        if (NavMesh.SamplePosition(
            randomDirection, out var hitLocation, wonderRadius, NavMesh.AllAreas))  // Find closest point in radius
        {
            agent.SetDestination(hitLocation.position);
            agent.speed = Random.Range(MinWonderSpeed, MaxWonderSpeed);
        }
        else if(RetryCoroutine == null)
            {
                 RetryCoroutine = StartCoroutine(RetryDelay()); 
            }
    }

    private IEnumerator RetryDelay()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;
            Debug.Log("Retrying to find a valid destination...");
            yield return null;
        }
        //yield return new WaitForSeconds(0.5f);
        RetryCoroutine = null;
        WanderToNextLocation();
    }
}
