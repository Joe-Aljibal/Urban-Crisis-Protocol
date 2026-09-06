using UnityEngine;
using UnityEngine.InputSystem;

public class PointerHandler : MonoBehaviour
{
    // PREFABS
    [SerializeField] public Grid grid;
    public GameObject currentGhost;
    public GameObject currentBuilding;
    public float cellSize;

    // STATES
    PointerBaseState currentState;
    public PointerPlacingstate placingState = new PointerPlacingstate();
    PointerInteractingState interactingState = new  PointerInteractingState();
    PointerInteractingState defaultState = new PointerInteractingState();

    public enum PointerState
    {
        Default,
        Interacting,
        Placing
    }

    public PointerState state;

    private void Start()
    {
        cellSize = grid.cellSize.x;
        ChangeState(placingState);
    }
    void Update()
    {
        currentState.UpdateState(this);
    }

    public void ChangeState(PointerBaseState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }
}