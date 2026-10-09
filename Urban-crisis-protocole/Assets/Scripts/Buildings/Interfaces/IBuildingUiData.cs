using UnityEngine;
using System.Collections.Generic;

public interface IBuildingUiData
{
    public List<UiDataText> GetUiTextList();
    public List<UiDataInteraction> GetUiInteractionList();
}

