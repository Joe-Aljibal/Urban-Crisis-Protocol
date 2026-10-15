using UnityEngine;
using UnityEngine.InputSystem;

public class TownCameraController : MonoBehaviour
{
    private enum CameraView
    {
        HumanTown,
        AiTown,
    }

    [SerializeField]
    private Transform humanTown;

    [SerializeField]
    private Transform aiTown;

    [SerializeField]
    private float pitch = 55f;

    [SerializeField]
    private float yaw = 45f;

    [SerializeField]
    private float dragSpeed = 1f;

    [SerializeField]
    private float speed = 30f;

    [SerializeField]
    private float zoomTransitionSpeed = 20f;

    [SerializeField]
    private float panLimit = 190f;

    [SerializeField]
    private float cameraDistance = 60f;

    [SerializeField]
    private float townZoom = 15f;

    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float minimumZoom = 8f;

    [SerializeField]
    private float maximumZoom = 20f;

    private Camera cameraComponent;
    private Vector3 panOffset;

    private bool dragging;

    private Vector2 previousMousePosition;
    private CameraView currentView;

    public Vector3 FocusPosition => GetCurrentTown().position + panOffset;

    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
        currentView = CameraView.HumanTown;

        SetCameraImmediately();
    }

    private void Update()
    {
        HandleCameraSwitchButtons();
        HandleZoom();
        HandleMouseDrag();
        MoveCamera();
    }

    private void HandleCameraSwitchButtons()
    {
        Keyboard keyboard = Keyboard.current;
        CameraView previousView = currentView;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            currentView = CameraView.HumanTown;
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            currentView = CameraView.AiTown;
        }
        else if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (currentView == CameraView.HumanTown)
            {
                currentView = CameraView.AiTown;
            }
            else
            {
                currentView = CameraView.HumanTown;
            }
        }

        if (currentView != previousView)
        {
            panOffset = Vector3.zero;
            dragging = false;
        }
    }

    private void HandleMouseDrag()
    {
        Mouse mouse = Mouse.current;

        Vector2 currentMousePosition = mouse.position.ReadValue();
        if (mouse.middleButton.wasPressedThisFrame)
        {
            dragging = true;
            previousMousePosition = currentMousePosition;
        }
        if (dragging && mouse.middleButton.isPressed)
        {
            Vector2 mouseDifference = currentMousePosition - previousMousePosition;
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            float worldUnitsPerPixel = cameraComponent.orthographicSize * 2f / Screen.height;
            MoveFocus(
                (right * mouseDifference.x + forward * mouseDifference.y)
                * worldUnitsPerPixel
                * -dragSpeed
            );

            previousMousePosition = currentMousePosition;
        }
        if (mouse.middleButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
    }

    private void HandleZoom()
    {
        Mouse mouse = Mouse.current;

        float mouseWheel = mouse.scroll.ReadValue().y;

        if (mouseWheel == 0f)
            return;

        if (mouseWheel > 0f)
        {
            townZoom -= zoomSpeed;
        }
        else
        {
            townZoom += zoomSpeed;
        }

        townZoom = Mathf.Clamp(townZoom, minimumZoom, maximumZoom);
    }

    private void MoveCamera()
    {
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition =
            FocusPosition - cameraRotation * Vector3.forward * cameraDistance;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        transform.rotation = cameraRotation;

        cameraComponent.orthographicSize = Mathf.MoveTowards(
            cameraComponent.orthographicSize,
            townZoom,
            zoomTransitionSpeed * Time.deltaTime
        );
    }

    private void SetCameraImmediately()
    {
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = cameraRotation;
        transform.position =
            FocusPosition - cameraRotation * Vector3.forward * cameraDistance;
        cameraComponent.orthographicSize = townZoom;
    }

    public void MoveFocus(Vector3 movement)
    {
        panOffset += movement;
        ClampPanOffset();
    }

    public void FocusOnGround(Vector3 groundPosition)
    {
        Vector3 humanDistance = groundPosition - humanTown.position;
        Vector3 aiDistance = groundPosition - aiTown.position;

        humanDistance.y = 0f;
        aiDistance.y = 0f;

        if (humanDistance.sqrMagnitude <= aiDistance.sqrMagnitude)
        {
            currentView = CameraView.HumanTown;
        }
        else
        {
            currentView = CameraView.AiTown;
        }

        panOffset = groundPosition - GetCurrentTown().position;
        panOffset.y = 0f;

        ClampPanOffset();
        dragging = false;
    }

    private Transform GetCurrentTown()
    {
        if (currentView == CameraView.HumanTown)
        {
            return humanTown;
        }

        return aiTown;
    }

    private void ClampPanOffset()
    {
        panOffset.x = Mathf.Clamp(panOffset.x, -panLimit, panLimit);
        panOffset.z = Mathf.Clamp(panOffset.z, -panLimit, panLimit);
    }
}
