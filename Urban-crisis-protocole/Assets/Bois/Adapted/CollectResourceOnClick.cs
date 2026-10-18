using UnityEngine;
using UnityEngine.InputSystem;

public class CollectResourceOnClick : MonoBehaviour
{
    private const int ResourceAmountPerClick = 1;

    [SerializeField] private Transform humanTown;
    [SerializeField] private Transform aiTown;

    PlayerInfo playerInfo = PlayerInfo.Instance;

    private void CollectResourceClick()
    {
        if (ResourceUIManager.IsPointerOverDashboard)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            StaticResource resource = hit.transform.GetComponentInParent<StaticResource>();

            if (resource == null || !resource.IsAvailable)
                return;

            TownType town = GetClosestTown(resource.transform.position);

            if (town == TownType.Human)
            {
                playerInfo.addResource(
                    resource.ResourceType,
                    ResourceAmountPerClick
                );
            }

            TownResourceStatistics.AddResource(
                town,
                resource.ResourceType,
                ResourceAmountPerClick
            );
            resource.Disable();
        }
    }

    private TownType GetClosestTown(Vector3 resourcePosition)
    {
        float distanceFromHumanTown = Vector3.SqrMagnitude(
            resourcePosition - humanTown.position
        );
        float distanceFromAiTown = Vector3.SqrMagnitude(
            resourcePosition - aiTown.position
        );

        if (distanceFromHumanTown <= distanceFromAiTown)
            return TownType.Human;

        return TownType.AI;
    }

    void Update()
    {
        CollectResourceClick();
    }

}
