using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ExplosionAreaController : MonoBehaviour
{
    [SerializeField]
    private float explosionRadius = 20f;

    [SerializeField]
    private float growthRadius = 1.5f;

    [SerializeField]
    private float triggerDuration = 0.2f;

    [SerializeField]
    private GameObject explosionSpherePrefab;

    [SerializeField]
    private Material explosionSphereMaterial;

    [SerializeField]
    private Color explosionSphereColor = new Color(1f, 0.45f, 0.1f);

    private BombDetonationController detonationController;
    private SphereCollider explosionCollider;
    private Transform explosionSphere;
    private Coroutine areaCoroutine;
    public event Action OnAreaFinished;

    private HashSet<IDestructible> destroyedObjects = new HashSet<IDestructible>();

    private void Start()
    {
        detonationController = GetComponent<BombDetonationController>();
        explosionCollider = GetComponent<SphereCollider>();

        explosionCollider.isTrigger = true;
        explosionCollider.enabled = false;

        SetupExplosionSphere();

        detonationController.OnDetonated += ActivateExplosionArea;
    }

    private void SetupExplosionSphere()
    {
        GameObject sphereObject;

        if (explosionSpherePrefab != null)
        {
            sphereObject = Instantiate(explosionSpherePrefab);
        }
        else
        {
            sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Collider primitiveCollider = sphereObject.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Destroy(primitiveCollider);
            }

            Renderer sphereRenderer = sphereObject.GetComponent<Renderer>();
            if (sphereRenderer != null)
            {
                sphereRenderer.material = CreateDefaultExplosionMaterial();
            }
        }

        sphereObject.name = "ExplosionVisualSphere";
        sphereObject.SetActive(false);
        explosionSphere = sphereObject.transform;
    }

    private Material CreateDefaultExplosionMaterial()
    {
        if (explosionSphereMaterial != null)
        {
            return explosionSphereMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = explosionSphereColor;

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", explosionSphereColor * 2f);
        }

        return material;
    }

    private void ActivateExplosionArea(Vector3 explosionPosition)
    {
        transform.position = explosionPosition;
        destroyedObjects.Clear();

        explosionCollider.radius = 0f;
        explosionCollider.enabled = true;

        if (explosionSphere != null)
        {
            explosionSphere.position = explosionPosition;
            explosionSphere.localScale = Vector3.zero;
            explosionSphere.gameObject.SetActive(true);
        }

        if (areaCoroutine != null)
            StopCoroutine(areaCoroutine);

        areaCoroutine = StartCoroutine(GrowExplosionArea());
    }

    private IEnumerator GrowExplosionArea()
    {
        float elapsed = 0f;

        while (elapsed < growthRadius)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growthRadius);
            float radius = Mathf.Lerp(0f, explosionRadius, t);

            explosionCollider.radius = radius;
            UpdateExplosionSphere(radius);

            Debug.Log(
                $"[Explosion] radius = {explosionCollider.radius:F2} / {explosionRadius} (t = {t:F2})"
            );

            yield return null;
        }

        explosionCollider.radius = explosionRadius;
        UpdateExplosionSphere(explosionRadius);

        yield return new WaitForSeconds(triggerDuration);
        explosionCollider.enabled = false;

        if (explosionSphere != null)
        {
            explosionSphere.gameObject.SetActive(false);
        }

        OnAreaFinished?.Invoke();
    }

    private void UpdateExplosionSphere(float radius)
    {
        if (explosionSphere == null)
            return;

        explosionSphere.localScale = Vector3.one * (radius * 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDestructible destructible = other.GetComponentInParent<IDestructible>();

        if (destructible == null)
        {
            return;
        }

        if (destroyedObjects.Contains(destructible))
        {
            return;
        }

        destroyedObjects.Add(destructible);
        destructible.DestroyObject();
    }

    private void OnDestroy()
    {
        detonationController.OnDetonated -= ActivateExplosionArea;

        if (explosionSphere != null)
        {
            Destroy(explosionSphere.gameObject);
        }
    }
}
