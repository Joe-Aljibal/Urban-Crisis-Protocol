using UnityEngine;

// Soldat d'une base militaire : patrouille autour de son poste quand la base est alimentee.
// La base appelle SetOnDuty() ; sans base (ex. prefab fantome), le soldat reste immobile.
public class Solider : MonoBehaviour
{
    [SerializeField] float patrolRadius = 3f;
    [SerializeField] float walkSpeed = 1.2f;
    [SerializeField] float turnSpeed = 360f;      // degres par seconde
    [SerializeField] Vector2 waitTime = new(1f, 3f); // pause min/max a chaque point

    // Tout est en coordonnees locales : la patrouille suit la base si elle est tournee/deplacee.
    Vector3 post;
    Vector3 target;
    float waitTimer;

    void Awake()
    {
        post = transform.localPosition;
        target = post;
        enabled = false; // pas d'Update tant que la base n'est pas active
    }

    public void SetOnDuty(bool onDuty)
    {
        enabled = onDuty;
        if (!onDuty)
        {
            // Retour au poste quand la base perd son electricite.
            transform.localPosition = post;
            target = post;
        }
    }

    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 position = transform.localPosition;
        Vector3 toTarget = target - position;

        if (toTarget.sqrMagnitude < 0.01f)
        {
            PickNextPoint();
            return;
        }

        transform.localPosition = Vector3.MoveTowards(position, target, walkSpeed * Time.deltaTime);

        Quaternion look = Quaternion.LookRotation(toTarget, Vector3.up);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, look, turnSpeed * Time.deltaTime);
    }

    void PickNextPoint()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        target = post + new Vector3(offset.x, 0, offset.y);
        waitTimer = Random.Range(waitTime.x, waitTime.y);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying && transform.parent != null
            ? transform.parent.TransformPoint(post)
            : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, patrolRadius);
    }
}
