using System.Collections.Generic;
using UnityEngine;

// Connects raids and the bomb to the actual buildings anywhere on the map.
public class CityLegacyEvents : MonoBehaviour
{
    [SerializeField] private Bandit banditPrefab;
    [SerializeField] private GameObject bombPrefab;
    private BanditRaidEvent raid;
    private BombSpawner bomb;
    private Transform eventRoot;
    private Transform targetRoot;
    private Transform raidEntrance;
    private Transform bombDropPoint;
    private readonly List<Transform> activeTargets = new List<Transform>();
    public static CityLegacyEvents Instance { get; private set; }
    public bool IsReady => raid != null && bomb != null;
    public bool RaidRunning => raid != null && raid.IsRunning;
    public string Status => raid != null ? raid.Status : "Preparing legacy events...";

    private void Awake() => Instance = this;

    private void Start()
    {
        if (banditPrefab == null || bombPrefab == null) return;
        eventRoot = new GameObject("City events").transform;
        targetRoot = Point("Raid targets", Vector3.zero);
        raidEntrance = Point("Raid entrance", Vector3.zero);
        raid = eventRoot.gameObject.AddComponent<BanditRaidEvent>();
        raid.Configure(banditPrefab, raidEntrance, new BanditRaidEvent.MoneyBuilding[0]);
        bomb = eventRoot.gameObject.AddComponent<BombSpawner>();
        bombDropPoint = Point("Bomb drop point", Vector3.up * 180f);
        bombDropPoint.rotation = Quaternion.Euler(180f, 0f, 0f);
        bomb.Configure(bombPrefab, bombDropPoint);
    }

    private Transform Point(string name, Vector3 position, Transform parent = null)
    {
        Transform point = new GameObject(name).transform;
        point.SetParent(parent != null ? parent : eventRoot, false);
        point.position = position;
        return point;
    }

    public void StartRaid()
    {
        if (raid == null || raid.IsRunning) return;
        var buildings = CityStructure.All.FindAll(building => building != null && !building.IsDestroyed);
        // Randomise before assigning the finite number of bandits so every building can be targeted.
        for (int i = buildings.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CityStructure item = buildings[i]; buildings[i] = buildings[j]; buildings[j] = item;
        }
        if (buildings.Count == 0) return;
        foreach (Transform point in targetRoot) Destroy(point.gameObject);
        activeTargets.Clear();
        var targets = new List<BanditRaidEvent.MoneyBuilding>();
        foreach (CityStructure building in buildings)
        {
            Vector3 position = building.transform.position;
            Transform inside = Point("Building target", position, targetRoot);
            Transform door = Point("Building entrance", position + Vector3.forward * 4f, targetRoot);
            Transform spawn = Point("Bandit approach", position + new Vector3(-25f, 0f, 6f), targetRoot);
            targets.Add(new BanditRaidEvent.MoneyBuilding { building = building.transform, approachPoints = new[] { door }, insidePoint = inside, spawnPoint = spawn });
            if (targets.Count <= raid.BanditCount) activeTargets.Add(building.transform);
        }
        raid.SetTargets(targets.ToArray());
        raid.StartRaid();
    }

    public void LaunchBomb()
    {
        if (bomb == null) return;
        CityStructure target = CityStructure.RandomIntact();
        if (target == null) return;
        if (raid != null && raid.IsRunning) raid.StopRaid();
        Vector3 location = target.transform.position;
        bombDropPoint.position = location + Vector3.up * 180f;
        FindAnyObjectByType<TownCameraController>()?.FocusOnGround(location);
        bomb.TrySpawn();
    }

    public bool DispatchSecurity()
    {
        if (!RaidRunning || PlayerInfo.Instance.getMoney < 100f) return false;
        PlayerInfo.Instance.addMoney(-100f);
        raid.StopRaid();
        return true;
    }

    public bool TryGetDanger(Vector3 point, out Vector3 center)
    {
        center = Vector3.zero;
        if (!RaidRunning) return false;
        foreach (Transform target in activeTargets)
        {
            if (target == null) continue;
            Vector3 delta = point - target.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 144f) { center = target.position; return true; }
        }
        return false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (eventRoot != null) Destroy(eventRoot.gameObject);
    }
}
