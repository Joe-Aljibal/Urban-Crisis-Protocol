using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionAreaController : MonoBehaviour
{
    [SerializeField]
    private float explosionRadius = 20f;

    [SerializeField]
    private float triggerDuration = 0.2f;

    private BombDetonationController detonationController;
    private SphereCollider explosionCollider;
    private Coroutine areaCoroutine;

    private HashSet<IDestructible> destroyedObjects = new HashSet<IDestructible>();

    private void Start()
    {
        detonationController = GetComponent<BombDetonationController>();
        explosionCollider = GetComponent<SphereCollider>();

        explosionCollider.isTrigger = true;
        explosionCollider.enabled = false;

        detonationController.OnDetonated += ActivateExplosionArea;
    }

    private void ActivateExplosionArea(Vector3 explosionPosition)
    {
        transform.position = explosionPosition;

        destroyedObjects.Clear();

        explosionCollider.radius = explosionRadius;
        explosionCollider.enabled = true;

        areaCoroutine = StartCoroutine(DisableExplosionArea());
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

    private IEnumerator DisableExplosionArea()
    {
        yield return new WaitForSeconds(triggerDuration);

        explosionCollider.enabled = false;
    }

    private void OnDestroy()
    {
        detonationController.OnDetonated -= ActivateExplosionArea;
    }
}
