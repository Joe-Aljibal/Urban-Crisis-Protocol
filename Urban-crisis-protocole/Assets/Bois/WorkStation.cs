using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkStation : MonoBehaviour
{
    protected GameObject npcPrefab;
    protected int workers = 0;
    public int Workers => workers;
    protected int buildingCapacity = 5;
    public int BuildingCapacity => buildingCapacity;

    protected virtual List<Worker> WorkerList { get; } = new List<Worker>();
    public event Action<int> OnNpcAssigned;
    
    public virtual void AssignNpcs(int number)
    {
        workers += number;
        OnNpcAssigned?.Invoke(number);

    }
    public virtual void RemoveNpcs(int number)
    {
        if(workers > number)
        {
        workers -= number;
        } else
        {
            workers = 0;
        }
    }

    public virtual void CreateNpcs(int number)
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 offsetPosition = new Vector3(
                transform.position.x + i,
                transform.position.y,
                transform.position.z);
            Worker worker = Instantiate(npcPrefab, offsetPosition, Quaternion.identity)
            .GetComponent<Worker>();
            WorkerList.Add(worker);
        }
    }
    public virtual void InitializeBuilding(Vector3 position, int workerCount, GameObject prefab)
    {
        transform.position = position;
        workers = workerCount;
        npcPrefab = prefab;
        CreateNpcs(workerCount);
    }
    public virtual void InitializeBuildingNoWorkers(Vector3 position)
    {
        transform.position = position;
    }

    public virtual void Update()
    {
        if(WorkerList.Count != workers)
        {
            if(workers > WorkerList.Count)
            {
                CreateNpcs(1);
            } else
            {
                WorkerList[0].Delete();
            }
        }
    }
}