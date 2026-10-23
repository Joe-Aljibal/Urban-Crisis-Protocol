using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildingSO
{
    public Sprite GetIcon();
    public BuildingCard GetCard();
    public bool CanPlace();
    public string GetBuildingName();
    public List<Dictionary<string, string>> GetStats();
    public string GetDescription();
}

