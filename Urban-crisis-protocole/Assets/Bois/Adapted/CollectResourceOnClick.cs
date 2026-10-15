using UnityEngine;
using UnityEngine.InputSystem;

public class CollectResourceOnClick : MonoBehaviour
{
    PlayerInfo playerInfo = PlayerInfo.Instance;

    private void CollectResourceClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            StaticResource resource = hit.transform.GetComponentInParent<StaticResource>();

            if (resource == null || !resource.IsAvailable)
                return;

            playerInfo.addResource(resource.ResourceType, 1);
            resource.Disable();
        }
    }

    void Update()
    {
        CollectResourceClick();
    }

}
