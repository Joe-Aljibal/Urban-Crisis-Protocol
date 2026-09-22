using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectResourceOnClick : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;
    private void CollectResourceClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out StaticResource r) && Mouse.current.leftButton.wasPressedThisFrame
            && r.IsAvailable)
            {
                playerInfo.addResource(r.ResourceType, 1);
            }
        }
    }

    void Update()
    {
        CollectResourceClick();
    }

}