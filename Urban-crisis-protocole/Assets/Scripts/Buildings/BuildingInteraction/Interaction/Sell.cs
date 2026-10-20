using UnityEngine;

public class Sell : IInteract
{
    public void Interact(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<ISellable>(out var sellableBuilding))
        {
            sellableBuilding.Sell();
        }
    }
}
