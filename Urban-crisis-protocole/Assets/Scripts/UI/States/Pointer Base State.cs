using UnityEngine;

public abstract class PointerBaseState
{
    public abstract void EnterState(PointerHandler pointer);
    public abstract void UpdateState(PointerHandler pointer);
}
