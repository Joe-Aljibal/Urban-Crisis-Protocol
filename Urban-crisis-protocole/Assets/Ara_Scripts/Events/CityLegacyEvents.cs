using System.Collections;
using System.Collections.Generic;
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
    private CityStructure raidTarget;
    private BanditRaidEvent.MoneyBuilding townTarget;
    private Transform constructionDoor;
    private Transform constructionInside;
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
        raidTarget = target;
        location = target.transform.position;
        location.y = 0f;
        eventRoot = new GameObject("Original city events").transform;
        Transform entrance = Point("Raid entrance", location + new Vector3(-25f, 0, 6f));
        Transform door = Point("Building entrance", location + new Vector3(0, 0, 4f));
        Transform inside = Point("Building interior", location);
        raid = eventRoot.gameObject.AddComponent<BanditRaidEvent>();
        townTarget = new BanditRaidEvent.MoneyBuilding { building = target.transform, approachPoints = new[] { door }, insidePoint = inside };
        constructionDoor = Point("Construction entrance", location);
        constructionInside = Point("Construction target", location);
        raid.Configure(banditPrefab, entrance, new[] { townTarget });
        bomb = eventRoot.gameObject.AddComponent<BombSpawner>();
        Transform bombDropPoint = Point("Bomb drop point", location + Vector3.up * 180f);
        bombDropPoint.rotation = Quaternion.Euler(180f, 0f, 0f);
        bomb.Configure(bombPrefab, bombDropPoint);
    }

    private Transform Point(string name, Vector3 position)
    {
        Transform point = new GameObject(name).transform;
        point.SetParent(eventRoot, false);
        point.position = position;
        return point;
    }

    public void StartRaid()
    {
        if (raid == null || raid.IsRunning) return;
        List<BanditRaidEvent.MoneyBuilding> targets = new List<BanditRaidEvent.MoneyBuilding>();
        if (raidTarget != null && !raidTarget.IsDestroyed) targets.Add(townTarget);
        CityStructure construction = CityStructure.All.Find(building => building != null && !building.IsDestroyed && building.District == "Construction");
        if (construction != null)
        {
            Vector3 position = construction.transform.position;
            position.y = 0f;
            constructionInside.position = position;
            constructionDoor.position = position + Vector3.forward * 4f;
            targets.Add(new BanditRaidEvent.MoneyBuilding { building = construction.transform, approachPoints = new[] { constructionDoor }, insidePoint = constructionInside });
        }
        if (targets.Count == 0) return;
        raid.SetTargets(targets.ToArray());
        raid.StartRaid();
    }
    public void LaunchBomb()
    {
        if (bomb == null || raidTarget == null || raidTarget.IsDestroyed) return;
        if (raid != null && raid.IsRunning) raid.StopRaid();
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
        center = location;
        if (!RaidRunning) return false;
        if ((point - location).sqrMagnitude < 144f) return true;
        if (constructionInside == null) return false;
        center = constructionInside.position;
        return (point - center).sqrMagnitude < 144f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (eventRoot != null) Destroy(eventRoot.gameObject);
    }
}
