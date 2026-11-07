using UnityEngine;
using UnityEngine.InputSystem;

public class TownMapController : MonoBehaviour
{
    [SerializeField]
    private TownCameraController townCamera;

    [SerializeField]
    private float keyboardSpeed = 20f;

    [SerializeField]
    private float mapMoveSpeed = 50f;

    private Camera mapCamera;

    private void Start()
    {
        mapCamera = GetComponent<Camera>();

        Vector3 focusPosition = townCamera.FocusPosition;
        focusPosition.y = transform.position.y;
        transform.position = focusPosition;
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMapClick();
        FollowCameraFocus();
    }

    private void HandleKeyboardMovement()
    {
        Keyboard keyboard = Keyboard.current;
        Vector2 input = Vector2.zero;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            input.x--;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            input.x++;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            input.y--;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            input.y++;

        if (input == Vector2.zero)
            return;

        Vector3 right = townCamera.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 forward = townCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 movement = (right * input.x + forward * input.y).normalized;
        townCamera.MoveFocus(movement * keyboardSpeed * Time.deltaTime);
    }

    private void HandleMapClick()
    {
        Mouse mouse = Mouse.current;

        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = mouse.position.ReadValue();

        if (!mapCamera.pixelRect.Contains(mousePosition))
            return;

        Ray ray = mapCamera.ScreenPointToRay(mousePosition);
        Plane ground = new Plane(Vector3.up, townCamera.FocusPosition);

        ground.Raycast(ray, out float distance);
        townCamera.FocusOnGround(ray.GetPoint(distance));
    }

    private void FollowCameraFocus()
    {
        Vector3 targetPosition = townCamera.FocusPosition;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            mapMoveSpeed * Time.deltaTime
        );
    }
}
