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
    private AudioClip explosionSound;

    [SerializeField]
    private float explosionVolume = 1f;

    [SerializeField]
    public float effectDuration = 5f;
    private bool hasExploded = false;

    public event Action<Vector3> OnDetonated;
    private Coroutine explosionCoroutine;
    private ExplosionAreaController explosionArea;

    private void Awake()
    {
        explosionArea = GetComponent<ExplosionAreaController>();
    }

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

        if (explosionArea != null) explosionArea.OnAreaFinished += DestroyBomb;
        OnDetonated?.Invoke(position);
        if (explosionArea == null) DestroyBomb();
    }

    private void CreateExplosionEffect(Vector3 position)
    {
        GameObject newEffect = Instantiate(explosionEffect, position, Quaternion.identity);

        // The explosion prefab also contains a looping falling trail. It belongs
        // to the descent, not to the explosion spawned after impact.
        Transform fallingEffect = newEffect.transform.Find("Falling Effects");
        if (fallingEffect != null) fallingEffect.gameObject.SetActive(false);

        foreach (ParticleSystem particles in newEffect.GetComponentsInChildren<ParticleSystem>())
        {
            // Emit one burst: a continuous stream looked like another falling effect.
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var settings = particles.main;
            settings.loop = false;
            var emission = particles.emission;
            emission.enabled = false;
            particles.Play();
            particles.Emit(particles.name == "ExplosionSmoke" ? 35 : 1);
        }
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, position, explosionVolume);
        }
        Destroy(newEffect, effectDuration);
    }

    private void DestroyBomb()
    {
        Destroy(gameObject, 0.5f);
    }
}
