using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PointerPlacingstate : PointerBaseState
{
    float cellSize;
    int placementRotation = 0;
    PlayerInfo playerInfo = PlayerInfo.Instance;

    public override void EnterState(PointerHandler pointer)
    {
        cellSize = pointer.cellSize;
    }

    public override void UpdateState(PointerHandler pointer)
    {
    }
}
