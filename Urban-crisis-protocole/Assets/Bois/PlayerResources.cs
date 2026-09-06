using System;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
     private int wood = 0;
     public int Wood => wood;

    public static Action<ResourceType> OnResourceCollected;
    public event Action OnWoodAdded;

    void Awake()
    {
        OnResourceCollected += HandleResourceCollected;
    }

    private void HandleResourceCollected(ResourceType resourceType)
    {
        switch (resourceType)
        {
            
            case ResourceType.Wood:
            wood++;
            OnWoodAdded?.Invoke();
            break;
        }
    }

}