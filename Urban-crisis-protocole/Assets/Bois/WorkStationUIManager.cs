using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorkStationUIManager : MonoBehaviour
{
    public event Action<Vector3, int> OnAddNpc;
    public event Action<Vector3, int, int> OnAddHutStation;

    public void AddWoodNpc(Vector3 position, int number)
    {
        OnAddNpc?.Invoke(position, number);
    }
    public void AddHutStation(Vector3 position, int number, int npcs)
    {
        OnAddHutStation?.Invoke(position, number, npcs);
    }

    void Update()
    {
        if(Keyboard.current.aKey.wasPressedThisFrame)
        {
            AddHutStation(new Vector3(0,1,0), 2, 1);
        }
    }
}
