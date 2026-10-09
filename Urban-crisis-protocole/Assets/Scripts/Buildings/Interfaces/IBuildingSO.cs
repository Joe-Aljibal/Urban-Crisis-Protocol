using UnityEngine;

public interface IBuildingSO
{
    public Sprite GetIcon();
    public BuildingCard GetCard();
    public bool CanPlace();
}

