using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class BombFallController : MonoBehaviour
{
    [SerializeField]
    private AudioClip fallingSound;

    [SerializeField]
    private AudioClip impactSound;
    private Rigidbody rb;
    private AudioSource fallSource;
    private AudioSource impactSource;

    [SerializeField]
    private LayerMask validImpactLayer;

    [SerializeField]
    private float impactLeadOffset = 0f;

    private bool isFalling;
    private bool impactSoundStarted;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        EnsureSources();
        BeginFall();
    }

    private void EnsureSources()
    {
        fallSource = GetComponent<AudioSource>();
        fallSource.playOnAwake = false;
        impactSource = gameObject.AddComponent<AudioSource>();
        impactSource.playOnAwake = false;
        fallSource.dopplerLevel = 0f;
        impactSource.dopplerLevel = 0f;
    }

    private void Update()
    {
        if (!IsFalling())
            return;
        if (impactSoundStarted)
            return;

        if (impactSound != null && GetTimeToImpact() <= impactSound.length + impactLeadOffset)
        {
            StartImpactSound();
        }
        else if (!fallSource.isPlaying && fallingSound != null)
        {
            fallSource.clip = fallingSound;
            fallSource.loop = true;
            fallSource.Play();
        }
    }

    private void StartImpactSound()
    {
        impactSoundStarted = true;
        impactSource.clip = impactSound;
        impactSource.loop = false;
        impactSource.Play();
        fallSource.Stop();
    }

    private float GetTimeToImpact()
    {
        if (
            !Physics.Raycast(
                transform.position,
                Vector3.down,
                out RaycastHit hit,
                Mathf.Infinity,
                validImpactLayer
            )
        )
        {
            return Mathf.Infinity;
        }

        float d = hit.distance;
        float v = GetFallSpeed();
        float g = Mathf.Abs(Physics.gravity.y);
        return (-v + Mathf.Sqrt(v * v + 2f * g * d)) / g;
    }

    private void BeginFall()
    {
        rb.useGravity = true;
        isFalling = true;
        impactSoundStarted = false;
        fallSource.clip = fallingSound;
        fallSource.loop = true;
        fallSource.Play();
    }

    private float GetFallSpeed()
    {
        return Mathf.Abs(rb.linearVelocity.y);
    }

    private bool IsFalling()
    {
        return isFalling;
    }

    public void StopFall()
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        isFalling = false;

        fallSource.Stop();
    }
}
