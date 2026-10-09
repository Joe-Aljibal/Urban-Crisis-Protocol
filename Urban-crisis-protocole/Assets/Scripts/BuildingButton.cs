using UnityEngine;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] private BuildingCard card;
    [SerializeField] private BuildingManager manager;
    public void OnClick()
    {
        manager.ChangeCurrentBuildingCard(card);
    }

}

