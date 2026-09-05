using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is the parent class of all buildings with workers. 
/// </summary>

public abstract class WorkStation : MonoBehaviour
{
    protected virtual GameObject NpcPrefab { get; set; }
    protected virtual List<Worker> WorkerList { get; } = new List<Worker>();

    protected int workers = 0;
    public int Workers => workers;
    protected int buildingCapacity = 5;
    public int BuildingCapacity => buildingCapacity;

    public virtual void InitializeBuilding(Vector3 position, int workerCount, GameObject prefab)
    {
        transform.position = position;
        NpcPrefab = prefab;
        AssignWorker(workerCount);
    }

    public void DestroyBuilding()
    {
        RemoveWorkers(workers);
        Destroy(gameObject);
    }

    public virtual void AssignWorker(int number)
    {
        workers += number;
        CreateWorker(number);

    }

    protected virtual Worker CreateSingleWorker(Vector3 position)
    {
        return Instantiate(NpcPrefab, position, Quaternion.identity)
            .GetComponent<Worker>();
    }

    protected virtual void CreateWorker(int number)
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 offsetPosition = new Vector3(
                transform.position.x + i,
                transform.position.y,
                transform.position.z);
            Worker worker = CreateSingleWorker(offsetPosition);
            WorkerList.Add(worker);
        }
        
    }

     public virtual void RemoveWorkers(int number)
    {
        if(workers > number)
        {
        DeleteWorkers(number);
        workers -= number;
        } else
        {
            DeleteWorkers(workers);
            workers = 0;
        }
    }

    protected void DeleteWorkers(int number)
    {
        for(int i = 0; i < number; i++)
        {
            Worker _worker = WorkerList[0];
            _worker.Delete();
            WorkerList.RemoveAt(0);
        }
    }
}