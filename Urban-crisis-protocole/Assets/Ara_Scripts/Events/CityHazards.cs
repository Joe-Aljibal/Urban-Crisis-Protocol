using UnityEngine;

// Shared query so pedestrians, collectors and traffic agree on dangerous areas.
public static class CityHazards
{
    public static bool TryGetDanger(Vector3 position, out Vector3 center)
    {
        center = Vector3.zero;
        return (CityFireEvent.Instance != null && CityFireEvent.Instance.TryGetDanger(position, out center)) ||
               (CityDisasterEvent.Instance != null && CityDisasterEvent.Instance.TryGetDanger(position, out center)) ||
               (CityLegacyEvents.Instance != null && CityLegacyEvents.Instance.TryGetDanger(position, out center));
    }
}
