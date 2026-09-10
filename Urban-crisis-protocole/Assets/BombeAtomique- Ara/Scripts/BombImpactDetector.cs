// detect when the bomb hits a valid object and trigger the next step
using UnityEngine;

public class BombImpactDetector : MonoBehaviour
{
    [SerializeField]
    private LayerMask validImpactLayer;
    private BombFallController fallController;
    private IDetonable detonationController;

    private void Start()
    {
        fallController = GetComponent<BombFallController>();
        detonationController = GetComponent<IDetonable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsValidImpact(collision))
        {
            HandleImpact(collision);
        }
    }

    private Vector3 GetImpactPoint(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            return transform.position;
        }
        return transform.position;
    }

    private bool IsValidImpact(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            return true;
        }

        IDestructible destructible = collision.gameObject.GetComponentInParent<IDestructible>();

        if (destructible != null)
        {
            return true;
        }

        return false;
    }

    private void HandleImpact(Collision collision)
    {
        Vector3 impactPoint = GetImpactPoint(collision);
        if (fallController != null)
        {
            fallController.StopFall();
        }
        NotifyDetonation(impactPoint);
    }

    private void NotifyDetonation(Vector3 impactPoint)
    {
        if (detonationController != null)
        {
            detonationController.Detonate(impactPoint);
        }
    }
}
