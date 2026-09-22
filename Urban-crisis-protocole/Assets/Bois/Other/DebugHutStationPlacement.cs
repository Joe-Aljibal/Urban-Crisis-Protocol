using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Only serves purpose of debugging
/// </summary>

public class DebugHutStationPlacement : MonoBehaviour
{
    [SerializeField] HutData[] hutDatas;
    private HutData currentHutData;

    void Start()
    {
        currentHutData = hutDatas[0];
    }
    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            int index = System.Array.IndexOf(hutDatas, currentHutData);
            index = (index + 1 ) % hutDatas.Length;
            currentHutData = hutDatas[index];
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            AddHut(new Vector3(0, 1, 0), currentHutData);
        }
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            AddWorker(new Vector3(0, 1, 0), 1);
        }
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            DeleteHut(new Vector3(0, 1, 0));
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            DeleteWorker(new Vector3(0, 1, 0), 1);
        }
        if (Keyboard.current.tabKey.isPressed)
        {
            Time.timeScale = 100.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }
    private void AddHut(Vector3 position, HutData hutData)
    {
        Instantiate(hutData.hutPrefab, position, Quaternion.identity);
    }

    private void AddWorker(Vector3 position, int number)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.AssignWorkers(number);
            }
        }
    }

    private void DeleteHut(Vector3 position)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.DestroyBuilding();
            }
        }
    }

    private void DeleteWorker(Vector3 position, int number)
    {
        float radius = 2f;

        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach (var col in hits)
        {

            if (col.TryGetComponent<WorkStation>(out var hut))
            {
                hut.RemoveWorkers(number);
            }
        }
    }
}
