using System;
using System.Collections;
using UnityEngine;

public class BombDetonationController : MonoBehaviour, IDetonable
{
    [SerializeField]
    private GameObject explosionEffect;

    [SerializeField]
    public float explosionDelay = 1f;

    [SerializeField]
    public float effectDuration = 5f;
    private bool hasExploded = false;

    public event Action<Vector3> OnDetonated;
    private Coroutine explosionCoroutine;

    public void Detonate(Vector3 position)
    {
        if (hasExploded)
        {
            return;
        }
        hasExploded = true;
        explosionCoroutine = StartCoroutine(ExplosionSequence(position));
    }

    private IEnumerator ExplosionSequence(Vector3 position)
    {
        yield return new WaitForSeconds(explosionDelay);
        CreateExplosionEffect(position);

        OnDetonated?.Invoke(position);
        DestroyBomb();
    }

    private void CreateExplosionEffect(Vector3 position)
    {
        GameObject newEffect = Instantiate(explosionEffect, position, Quaternion.identity);

        Destroy(newEffect, effectDuration);
    }

    private void DestroyBomb()
    {
        Destroy(gameObject, 0.5f);
    }
}
