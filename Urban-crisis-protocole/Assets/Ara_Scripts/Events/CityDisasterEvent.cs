using System.Collections.Generic;
using UnityEngine;

// Natural disasters run independently of the existing fire and blackout events.
public class CityDisasterEvent : MonoBehaviour
{
    private enum Phase { Waiting, Warning, Active, Recovery }
    public enum Disaster { Earthquake, Tornado, Flood, Thunderstorm, HeatWave }

    [SerializeField] private float initialDelay = 110f;
    [SerializeField] private float warningTime = 10f;
    [SerializeField] private float activeTime = 25f;
    [SerializeField] private float cooldown = 180f;
    [SerializeField] private float dangerRadius = 15f;
    [SerializeField] private float repairCost = 150f;
    private readonly List<CityStructure> damaged = new List<CityStructure>();
    private Phase phase;
    private Disaster disaster;
    private float timer;
    private float damageTimer;
    private Vector3 location;
    private Vector3 direction;
    private GameObject effect;
    private bool mitigated;
    public bool CanRespond => (phase == Phase.Warning || phase == Phase.Active) && !mitigated;
    public float ExposureMultiplier => mitigated ? 0.25f : 1f;
    private TownCameraController townCamera;

    public static CityDisasterEvent Instance { get; private set; }
    public bool HasIncident => phase != Phase.Waiting;
    public bool NeedsRepair => phase == Phase.Recovery;
    public float RepairCost => repairCost;
    public string Status { get; private set; } = "No natural disaster.";
    public Vector3 Location => location;

    private void Awake()
    {
        Instance = this;
        timer = initialDelay;
    }

    private void Start() => townCamera = FindFirstObjectByType<TownCameraController>();

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f) return;
        if (phase == Phase.Recovery) return;
        timer -= deltaTime;
        if (phase == Phase.Active)
        {
            if (disaster == Disaster.Tornado)
            {
                location += direction * (2f * deltaTime);
                if (effect != null) effect.transform.position = location;
            }
            damageTimer += deltaTime;
            if (damageTimer >= 1f)
            {
                DamageNearby(damageTimer);
                damageTimer = 0f;
            }
        }
        if (timer > 0f) return;
        if (phase == Phase.Waiting) BeginWarning();
        else if (phase == Phase.Warning) BeginDisaster();
        else if (phase == Phase.Active) FinishDisaster();
    }

    private void BeginWarning()
    {
        if (CityStructure.All.Count == 0)
        {
            timer = cooldown;
            return;
        }
        CityStructure building = CityStructure.All[Random.Range(0, CityStructure.All.Count)];
        if (building == null) { timer = cooldown; return; }
        TryStart((Disaster)Random.Range(0, 5), building.transform.position);
    }

    public bool TryStart(Disaster type, Vector3 center)
    {
        if (HasIncident) return false;
        location = center;
        location.y = 0f;
        disaster = type;
        mitigated = false;
        direction = Random.value < 0.5f ? Vector3.right : Vector3.left;
        damaged.Clear();
        phase = Phase.Warning;
        timer = warningTime;
        Status = disaster + " warning. Prepare the affected area.";
        return true;
    }

    public bool DispatchResponse()
    {
        if (!CanRespond || PlayerInfo.Instance.getMoney < 100f) return false;
        PlayerInfo.Instance.addMoney(-100f);
        mitigated = true;
        Status = disaster + ": emergency preparations reduce damage and exposure.";
        return true;
    }

    private void BeginDisaster()
    {
        phase = Phase.Active;
        timer = activeTime;
        damageTimer = 0f;
        Status = disaster + " active: evacuate the affected area.";
        if (disaster == Disaster.Thunderstorm) GetComponent<CityPowerOutage>()?.TriggerOutage();
        effect = new GameObject(disaster + " effect");
        effect.transform.position = location;
        effect.AddComponent<CityDisasterVisual>().Configure(disaster, dangerRadius);
    }

    private void DamageNearby(float elapsed)
    {
        if (disaster == Disaster.HeatWave) return; // Heat harms citizens, not structural integrity.
        foreach (CityStructure building in CityStructure.All)
        {
            if (building == null) continue;
            Vector3 offset = building.transform.position - location;
            offset.y = 0f;
            if (offset.sqrMagnitude > dangerRadius * dangerRadius) continue;
            building.Damage((disaster == Disaster.Thunderstorm ? 1f : 2f) * elapsed * ExposureMultiplier);
            if (!damaged.Contains(building)) damaged.Add(building);
        }
    }

    private void FinishDisaster()
    {
        if (effect != null) Destroy(effect);
        if (damaged.Count == 0)
        {
            phase = Phase.Waiting;
            timer = cooldown;
            Status = disaster + " ended without building damage.";
            return;
        }
        phase = Phase.Recovery;
        Status = disaster + " ended. " + damaged.Count + " buildings need repairs.";
    }

    public bool TryGetDanger(Vector3 point, out Vector3 center)
    {
        center = location;
        Vector3 offset = point - location;
        offset.y = 0f;
        return phase == Phase.Active && offset.sqrMagnitude < dangerRadius * dangerRadius;
    }

    public bool IsRoadBlocked(Vector3 point)
    {
        if (phase != Phase.Recovery || (disaster != Disaster.Earthquake && disaster != Disaster.Flood)) return false;
        Vector3 offset = point - location;
        offset.y = 0;
        return offset.sqrMagnitude < dangerRadius * dangerRadius;
    }

    public void Focus()
    {
        if (townCamera != null) townCamera.FocusOnGround(location);
    }

    public bool Repair()
    {
        if (phase != Phase.Recovery || PlayerInfo.Instance.getMoney < repairCost) return false;
        PlayerInfo.Instance.addMoney(-repairCost);
        foreach (CityStructure building in damaged)
            if (building != null) building.Repair();
        damaged.Clear();
        phase = Phase.Waiting;
        timer = cooldown;
        Status = "Disaster recovery complete.";
        return true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (effect != null) Destroy(effect);
    }
}
