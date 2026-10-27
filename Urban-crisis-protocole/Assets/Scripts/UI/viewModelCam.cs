using System.Collections;
using UnityEngine;

public class ViewModelCam : MonoBehaviour
{
    public GameObject model;
    Camera cam;
    [SerializeField] float padding = 1.2f;
    public float speed = 1f;
    public float height = 0f;
    public float distance = .5f;

    Coroutine spinningCoroutine;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void StartSpinning()
    {
        StopSpinning();
        spinningCoroutine = StartCoroutine(SpinningCoroutine());
    }

    public void StopSpinning()
    {
        if (spinningCoroutine != null)
        {
            StopCoroutine(spinningCoroutine);
            spinningCoroutine = null;
        }

        Vector3 center = GetModelCenter();
        transform.position = center + new Vector3(0, height, -distance);
        transform.LookAt(center);
    }

    Vector3 GetModelCenter()
    {
        if (model == null) return transform.position;

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return model.transform.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds.center;
    }

    IEnumerator SpinningCoroutine()
    {
        Vector3 center = GetModelCenter();

        Vector3 offset = transform.position - center;
        float angle = Mathf.Atan2(offset.z, offset.x);

        while (true)
        {
            center = GetModelCenter();

            angle += speed * Time.deltaTime;

            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;

            transform.position = center + new Vector3(x, height, z);
            transform.LookAt(center);
            yield return null;
        }
    }

    public void Frame(GameObject model)
    {
        if (!TryGetBounds(model, out Bounds bounds)) return;

        float radius = bounds.extents.magnitude;

        float vFov = cam.fieldOfView * Mathf.Deg2Rad;
        float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov / 2f) * cam.aspect);
        float limitingFov = Mathf.Min(vFov, hFov);

        float distance = (radius / Mathf.Sin(limitingFov / 2f)) * padding;

        cam.transform.position = bounds.center - cam.transform.forward * distance;
        cam.transform.LookAt(bounds.center);

        cam.nearClipPlane = Mathf.Max(0.01f, distance - radius * 1.5f);
        cam.farClipPlane = distance + radius * 3f;
    }

    private static bool TryGetBounds(GameObject obj, out Bounds bounds)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        bounds = default;
        if (renderers.Length == 0) return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return true;
    }
}