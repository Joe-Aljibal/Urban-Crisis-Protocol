using UnityEngine;
using System.Collections.Generic;

public interface IBuildingUiData
{
    public List<UiDataModel> GetUiDataList();
}


public static class BuildingUiDataCompatibility
{
    public static List<UiDataText> GetUiTextList(this IBuildingUiData building)
    {
        var result = new List<UiDataText>();
        foreach (var item in building.GetUiDataList()) result.Add(new UiDataText(item.GetUiDataType, item.GetValue));
        return result;
    }
    public static List<UiDataInteraction> GetUiInteractionList(this IBuildingUiData building)
    {
        var result = new List<UiDataInteraction>();
        if (building is ISellable) result.Add(new UiDataInteraction(UiInteractionType.SELL, new Sell()));
        return result;
    }
}
