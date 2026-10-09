using UnityEngine;

public class UiDataInteraction 
{
    private UiInteractionType type;
    public UiDataInteraction(UiInteractionType type)
    { this.type = type;}

    public UiInteractionType GetUiDataType => type;

}
