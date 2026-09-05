using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class will serve as a medium between the UI and the work station management logig.
/// The methods will be called directly by buttons from the UI. 
/// (The current inputs wil be replaced by buttons)
/// </summary>

public class WorkStationUIManager : MonoBehaviour
{
    public event Action<Vector3, int> OnAddNpc;
    public event Action<Vector3, int, int> OnAddHutStation;
    public event Action<Vector3> OnDeleteHutStation;
    public event Action<Vector3, int> OnDeleteWoodNpc;

    public void AddWoodNpc(Vector3 position, int number)
    {
        OnAddNpc?.Invoke(position, number);
    }
    public void AddHutStation(Vector3 position, int number, int npcs)
    {
        OnAddHutStation?.Invoke(position, number, npcs);
    }
    public void DeleteHutStation(Vector3 position)
    {
        OnDeleteHutStation?.Invoke(position);
    }
    public void DeleteWoodNpc(Vector3 position, int number)
    {
        OnDeleteWoodNpc?.Invoke(position, number);
    }

    void Update()
    {
        if(Keyboard.current.wKey.wasPressedThisFrame)
        {
            AddHutStation(new Vector3(0,1,0), 1, 1);
        }
        if(Keyboard.current.aKey.wasPressedThisFrame)
        {
            AddWoodNpc(new Vector3(0,1,0), 1);
        }
        if(Keyboard.current.dKey.wasPressedThisFrame)
        {
            DeleteWoodNpc(new Vector3(0,1,0), 1);
        }
        if(Keyboard.current.sKey.wasPressedThisFrame)
        {
            DeleteHutStation(new Vector3(0,1,0));
        }
        if (Keyboard.current.tabKey.isPressed)
        {
            Time.timeScale = 5.0f;
        } else
        {
            Time.timeScale = 1.0f;
        }
    }
}
