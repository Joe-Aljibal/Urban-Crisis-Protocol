using UnityEngine;

// One vehicle per loop, keeping the traffic prototype inexpensive and collision-free.
public class CityTraffic : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    private Vector3[] route;
    private int next;
    private float cornerPause;

    public void Configure(Vector3[] points)
    {
        route = points;
        transform.position = points[0];
        next = 1;
    }

    private void Update()
    {
        if (route == null || route.Length < 2) return;
        // Stop before entering a closed road segment and resume once it is safe.
        Vector3 lookAhead = Vector3.MoveTowards(transform.position, route[next], 5f);
        if (CityDisasterEvent.Instance != null && CityDisasterEvent.Instance.IsRoadBlocked(lookAhead)) return;
        if (CityHazards.TryGetDanger(lookAhead, out _) || CityHazards.TryGetDanger(transform.position, out _))
            return;
        if (cornerPause > 0f)
        {
            cornerPause -= Time.deltaTime;
            return;
        }
        Vector3 direction = route[next] - transform.position;
        if (direction.sqrMagnitude < 0.04f)
        {
            next = (next + 1) % route.Length;
            cornerPause = 0.6f;
            return;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
        transform.position = Vector3.MoveTowards(transform.position, route[next], speed * Time.deltaTime);
    }
}
