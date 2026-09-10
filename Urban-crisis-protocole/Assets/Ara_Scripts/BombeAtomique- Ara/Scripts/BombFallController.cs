using UnityEngine;

public class BombFallController : MonoBehaviour
{
    [SerializeField]
    private AudioClip fallingSound;

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isFalling;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        BeginFall();
    }

    private void BeginFall()
    {
        rb.useGravity = true;
        isFalling = true;
        audioSource.clip = fallingSound;
        audioSource.loop = true;
        audioSource.Play();
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
        audioSource.Stop();
    }
}
