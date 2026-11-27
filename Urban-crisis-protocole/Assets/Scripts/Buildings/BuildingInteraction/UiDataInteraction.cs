using UnityEngine;

public class UiDataInteraction 
{
    private UiInteractionType type;
    private IInteract interaction;
    public UiDataInteraction(UiInteractionType type, IInteract interaction)
    { 
        this.type = type;
        this.interaction = interaction;
    }

    public UiInteractionType GetUiInteractionType => type;
    public IInteract GetUiInteraction => interaction;

}
