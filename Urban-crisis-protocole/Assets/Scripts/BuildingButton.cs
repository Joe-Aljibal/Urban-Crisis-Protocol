using UnityEngine;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] private BuildingCard card;
    [SerializeField] private GameObject gameManager;
 private PlayerController controller;

    void Start()
    {
        controller = gameManager.GetComponent<PlayerController>();
    }
    
    public void OnClick()
    {
        controller.ChangeCurrentBuildingCard(card);
    }

}

