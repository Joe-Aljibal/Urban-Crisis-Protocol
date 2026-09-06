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
    public event Action<Vector3, HutType, int, int> OnAddHutStation;
    public event Action<Vector3, int> OnAddWorker;
    public event Action<Vector3> OnDeleteHutStation;
    public event Action<Vector3, int> OnDeleteWorker;

    public void AddHutStation(Vector3 position, HutType hutType, int number, int workers)
    {
        OnAddHutStation?.Invoke(position, hutType, number, workers);
    }

    public void AddWorker(Vector3 position, int number)
    {
        OnAddWorker?.Invoke(position, number);
    }

    public void DeleteHutStation(Vector3 position)
    {
        OnDeleteHutStation?.Invoke(position);
    }

    public void DeleteWorker(Vector3 position, int number)
    {
        OnDeleteWorker?.Invoke(position, number);
    }

    void Update()
    {
        if(Keyboard.current.wKey.wasPressedThisFrame)
        {
            AddHutStation(new Vector3(0,1,0), 0, 1, 1);
        }
        if(Keyboard.current.aKey.wasPressedThisFrame)
        {
            AddWorker(new Vector3(0,1,0), 1);
        }
        if(Keyboard.current.sKey.wasPressedThisFrame)
        {
            DeleteHutStation(new Vector3(0,1,0));
        }
        if(Keyboard.current.dKey.wasPressedThisFrame)
        {
            DeleteWorker(new Vector3(0,1,0), 1);
        }
        if (Keyboard.current.tabKey.isPressed)
        {
            Time.timeScale = 100.0f;
        } else
        {
            Time.timeScale = 1.0f;
        }
    }
}
