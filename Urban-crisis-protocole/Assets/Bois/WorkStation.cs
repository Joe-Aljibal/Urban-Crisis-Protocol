using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkStation : MonoBehaviour
{
    /* This class is the parent class of all buildings with workers. */

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
        AssignNpcs(workerCount);
    }

    public void DestroyBuilding()
    {
        RemoveNpcs(workers);
        Destroy(gameObject);
    }

    public virtual void AssignNpcs(int number)
    {
        workers += number;
        CreateNpcs(number);

    }

    protected virtual Worker CreateSingleNpc(Vector3 position)
    {
        return Instantiate(NpcPrefab, position, Quaternion.identity)
            .GetComponent<Worker>();
    }

    protected virtual void CreateNpcs(int number)
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 offsetPosition = new Vector3(
                transform.position.x + i,
                transform.position.y,
                transform.position.z);
            Worker worker = CreateSingleNpc(offsetPosition);
            WorkerList.Add(worker);
        }
        
    }

     public virtual void RemoveNpcs(int number)
    {
        if(workers > number)
        {
        DeleteNpcs(number);
        workers -= number;
        } else
        {
            DeleteNpcs(workers);
            workers = 0;
        }
    }

    protected void DeleteNpcs(int number)
    {
        for(int i = 0; i < number; i++)
        {
            Worker _worker = WorkerList[0];
            _worker.Delete();
            WorkerList.RemoveAt(0);
        }
    }
}