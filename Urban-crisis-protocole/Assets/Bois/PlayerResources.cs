using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void CollectWood()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out StaticResource r) && Mouse.current.leftButton.wasPressedThisFrame
            && r.IsAvailable)
            {
                r.Disable();
                r.IsAvailable = false;
                wood++;
                OnWoodAdded?.Invoke();
            }
        }
    }

    void Update()
    {
        CollectWood();
    }

}