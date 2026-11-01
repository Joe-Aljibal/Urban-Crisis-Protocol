using UnityEngine;
using UnityEngine.InputSystem;

public class CamMouvement : MonoBehaviour
{
    [SerializeField] private float minHeightCam = 4f;
    [SerializeField] private float maxHeightCam = 8f;
    [SerializeField] private float zoomStep = 0.5f;
    [SerializeField] private float rotateSpeed = 0.2f;
    [SerializeField] private float groundHeight = 0f;

    private Camera cam;
    private Vector3 dragAnchor;
    private bool isDragging;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Zoom(mouse.scroll.ReadValue().y);
        Drag(mouse);
        Orbit(mouse);
    }

    private bool TryGetGroundPoint(Vector2 screenPoint, out Vector3 point)
    {
        Plane ground = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
        Ray ray = cam.ScreenPointToRay(screenPoint);

        if (ground.Raycast(ray, out float distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        point = default;
        return false;
    }

    private void Zoom(float scroll)
    {
        if (Mathf.Approximately(scroll, 0f)) return;

        Vector3 target = transform.position + transform.forward * (Mathf.Sign(scroll) * zoomStep);

        if (target.y >= minHeightCam && target.y <= maxHeightCam)
        {
            transform.position = target;
        }
    }

    private void Drag(Mouse mouse)
    {
        Vector2 cursor = mouse.position.ReadValue();

        if (mouse.rightButton.wasPressedThisFrame)
        {
            isDragging = TryGetGroundPoint(cursor, out dragAnchor);
        }

        if (mouse.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging && mouse.rightButton.isPressed && TryGetGroundPoint(cursor, out Vector3 current))
        {
            transform.position += dragAnchor - current;
        }
    }

    private void Orbit(Mouse mouse)
    {
        if (!mouse.middleButton.isPressed) return;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        if (TryGetGroundPoint(center, out Vector3 pivot))
        {
            float angle = mouse.delta.ReadValue().x * rotateSpeed;
            transform.RotateAround(pivot, Vector3.up, angle);
        }
    }
}