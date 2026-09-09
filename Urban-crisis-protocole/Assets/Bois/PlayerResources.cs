using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerResources : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
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
                playerInfo.addWood(1);
            }
        }
    }

    void Update()
    {
        CollectWood();
    }

}