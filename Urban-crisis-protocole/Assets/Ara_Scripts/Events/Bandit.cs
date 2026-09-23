using System.Collections;
using UnityEngine;

public class Bandit : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 6;

    [SerializeField]
    private float timeInside = 2;

    [SerializeField]
    private GameObject model;

    [SerializeField]
    private GameObject moneyBag;

    [SerializeField]
    private Transform leftLeg;

    [SerializeField]
    private Transform rightLeg;

    public void BeginRaid(
        BanditRaidEvent raid,
        BanditRaidEvent.MoneyBuilding target,
        Vector3 escapePoint
    )
    {
        moneyBag.SetActive(false);

        StartCoroutine(RobBuilding(raid, target, escapePoint));
    }

    private IEnumerator RobBuilding(
        BanditRaidEvent raid,
        BanditRaidEvent.MoneyBuilding target,
        Vector3 escapePoint
    )
    {
        // Marcher jusqu'à la porte.
        foreach (Transform point in target.approachPoints)
        {
            yield return WalkTo(point.position);
        }

        // Entrer dans la banque.
        yield return WalkTo(target.insidePoint.position);

        model.SetActive(false);

        yield return new WaitForSeconds(timeInside);

        // Voler puis ressortir avec le sac.
        float stolenMoney = raid.StealMoney(target);

        moneyBag.SetActive(stolenMoney > 0);
        model.SetActive(true);

        // Reprendre le chemin dans l'autre sens.
        for (int i = target.approachPoints.Length - 1; i >= 0; i--)
        {
            yield return WalkTo(target.approachPoints[i].position);
        }

        yield return WalkTo(escapePoint);

        raid.BanditEscaped();
        Destroy(gameObject);
    }

    private IEnumerator WalkTo(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            transform.LookAt(destination);

            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                moveSpeed * Time.deltaTime
            );

            // Petite animation des jambes.
            float angle = Mathf.Sin(Time.time * 12) * 22;

            leftLeg.localRotation = Quaternion.Euler(angle, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-angle, 0, 0);

            yield return null;
        }

        transform.position = destination;

        leftLeg.localRotation = Quaternion.identity;
        rightLeg.localRotation = Quaternion.identity;
    }
}
