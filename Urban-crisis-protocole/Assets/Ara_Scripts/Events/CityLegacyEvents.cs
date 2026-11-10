using System.Collections;
using UnityEngine;

// Connects the original raid and bomb prefabs to the new world's structures.
public class CityLegacyEvents : MonoBehaviour
{
    [SerializeField] private Bandit banditPrefab;
    [SerializeField] private GameObject bombPrefab;
    private BanditRaidEvent raid;
    private BombSpawner bomb;
    private Transform eventRoot;
    private Vector3 location;
    public static CityLegacyEvents Instance { get; private set; }
    public bool IsReady => raid != null && bomb != null;
    public bool RaidRunning => raid != null && raid.IsRunning;
    public string Status => raid != null ? raid.Status : "Preparing legacy events...";

    private void Awake() => Instance = this;

    private IEnumerator Start()
    {
        // Allow the neighborhood builder to finish creating its target buildings.
        yield return null;
        CityStructure target = CityStructure.All.Find(building => building != null && building.District == "Commercial");
        if (target == null || banditPrefab == null || bombPrefab == null) yield break;
        location = target.transform.position;
        location.y = 0f;
        eventRoot = new GameObject("Original city events").transform;
        Transform entrance = Point("Raid entrance", location + new Vector3(-25f, 0, 6f));
        Transform door = Point("Building entrance", location + new Vector3(0, 0, 4f));
        Transform inside = Point("Building interior", location);
        raid = eventRoot.gameObject.AddComponent<BanditRaidEvent>();
        raid.Configure(banditPrefab, entrance, new[] {
            new BanditRaidEvent.MoneyBuilding { building = target.transform, approachPoints = new[] { door }, insidePoint = inside }
        });
        bomb = eventRoot.gameObject.AddComponent<BombSpawner>();
        bomb.Configure(bombPrefab, Point("Bomb drop point", location + Vector3.up * 45f));
    }

    private Transform Point(string name, Vector3 position)
    {
        Transform point = new GameObject(name).transform;
        point.SetParent(eventRoot, false);
        point.position = position;
        return point;
    }

    public void StartRaid() { if (raid != null) raid.StartRaid(); }
    public void LaunchBomb() { if (bomb != null) bomb.TrySpawn(); }

    public bool DispatchSecurity()
    {
        if (!RaidRunning || PlayerInfo.Instance.getMoney < 100f) return false;
        PlayerInfo.Instance.addMoney(-100f);
        raid.StopRaid();
        return true;
    }

    public bool TryGetDanger(Vector3 point, out Vector3 center)
    {
        center = location;
        return RaidRunning && (point - location).sqrMagnitude < 144f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (eventRoot != null) Destroy(eventRoot.gameObject);
    }
}
