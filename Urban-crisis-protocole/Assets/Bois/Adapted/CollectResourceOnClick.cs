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
        if (ResourceUIManager.IsPointerOverDashboard || CityFireEvent.IsPointerOverPanel || CityConstruction.IsPointerOverPanel)
            return;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        foreach (RaycastHit hit in hits)
        {
            StaticResource resource = hit.transform.GetComponentInParent<StaticResource>();
            // A worker reservation must not prevent the player from collecting it.
            if (resource == null || !resource.IsActiveResource) continue;

            TownType town = GetClosestTown(resource.transform.position);

            if (town == TownType.Human)
            {
                playerInfo.collectResource(
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
            return;
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
