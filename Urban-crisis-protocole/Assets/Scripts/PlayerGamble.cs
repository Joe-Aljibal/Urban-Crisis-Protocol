using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerGamble : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask clickableLayers = Physics.DefaultRaycastLayers;
    [SerializeField] private float raycastDistance = 1000f;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (playerCamera == null)
        {
            Debug.LogWarning("PlayerGamble needs a camera, and no MainCamera was found.", this);
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, clickableLayers))
            return;

        GambleLogic gamble = hit.collider.GetComponentInParent<GambleLogic>();
        if (gamble != null)
            gamble.OpenPanel();
    }
}
