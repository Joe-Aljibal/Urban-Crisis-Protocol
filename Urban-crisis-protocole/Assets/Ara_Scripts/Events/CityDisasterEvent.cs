using System.Collections.Generic;
using System.Collections;
using UnityEngine;

// Natural disasters run independently of the existing fire and blackout events.
public class CityDisasterEvent : MonoBehaviour
{
    private enum Phase { Waiting, Warning, Active, Recovery }
    public enum Disaster { Earthquake, Tornado, Thunderstorm }

    [SerializeField] private float initialDelay = 110f;
    [SerializeField] private float warningTime = 10f;
    [SerializeField] private float activeTime = 25f;
    [SerializeField] private float cooldown = 180f;
    [SerializeField] private float dangerRadius = 15f;
    [SerializeField] private float repairCost = 150f;
    [SerializeField] private GameObject tornadoPrefab;
    private Vector3 tornadoStart;
    private Vector3 tornadoEnd;
    private Vector3 tornadoScale;
    public int LightningStrikeCount { get; private set; }
    private readonly List<CityStructure> damaged = new List<CityStructure>();
    private Phase phase;
    private Disaster disaster;
    private float timer;
    private float damageTimer;
    private Vector3 location;
    private Vector3 direction;
    private GameObject effect;
    private GameObject constructionEffect;
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
                Vector3 previousLocation = location;
                location = Vector3.MoveTowards(location, tornadoEnd, 8f * deltaTime);
                DestroyResourcesAlongPath(previousLocation, location);
                if (effect != null)
                {
                    effect.transform.position = location;
                    effect.transform.Rotate(Vector3.up, 150f * deltaTime);
                    float distance = Vector3.Distance(tornadoStart, tornadoEnd);
                    float progress = distance > 0f ? 1f - Vector3.Distance(location, tornadoEnd) / distance : 1f;
                    effect.transform.localScale = tornadoScale * Mathf.Lerp(1f, 1.08f, progress);
                }
                if (Vector3.Distance(location, tornadoEnd) <= 0.1f) timer = 0f;
            }
            if (disaster == Disaster.Tornado) DamageNearby(deltaTime);
            else
            {
                damageTimer += deltaTime;
                if (damageTimer >= 1f)
                {
                    DamageNearby(damageTimer);
                    damageTimer = 0f;
                }
            }
        }
        if (timer > 0f) return;
        if (phase == Phase.Waiting) BeginWarning();
        else if (phase == Phase.Warning) BeginDisaster();
        else if (phase == Phase.Active) FinishDisaster();
    }

    private void BeginWarning()
    {
        List<CityStructure> availableBuildings = CityStructure.All.FindAll(building => building != null && !building.IsDestroyed);
        if (availableBuildings.Count == 0)
        {
            timer = cooldown;
            return;
        }
        CityStructure building = availableBuildings[Random.Range(0, availableBuildings.Count)];
        if (building == null) { timer = cooldown; return; }
        TryStart((Disaster)Random.Range(0, 3), building.transform.position);
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
        if (disaster == Disaster.Thunderstorm)
        {
            GetComponent<CityPowerOutage>()?.TriggerOutage();
            LightningStrikeCount = 0;
            Status = "Thunderstorm: three lightning strikes. Seek shelter!";
            StartCoroutine(LightningStrikes());
        }
        if (disaster == Disaster.Tornado && tornadoPrefab != null)
        {
            // Same straight crossing, rotation and growth as the original event.
            GameObject town = GameObject.Find("Human Town Center");
            CityConstruction construction = FindAnyObjectByType<CityConstruction>();
            Vector3 townCenter = town != null ? town.transform.position : location;
            townCenter.y = 0f;
            tornadoStart = townCenter + Vector3.forward * 18f;
            tornadoEnd = construction != null ? construction.BuildAreaPosition + Vector3.back * 10f : townCenter + Vector3.back * 65f;
            tornadoEnd.y = 0f;
            location = tornadoStart;
            effect = Instantiate(tornadoPrefab, location, Quaternion.identity);
            tornadoScale = effect.transform.localScale;
        }
        else
        {
            effect = new GameObject(disaster + " effect");
            effect.transform.position = location;
            effect.AddComponent<CityDisasterVisual>().Configure(disaster, dangerRadius);
            if (disaster == Disaster.Earthquake)
            {
                CityConstruction construction = FindAnyObjectByType<CityConstruction>();
                if (construction != null)
                {
                    constructionEffect = new GameObject("Construction earthquake");
                    constructionEffect.transform.position = construction.BuildAreaPosition;
                    constructionEffect.AddComponent<CityDisasterVisual>().Configure(disaster, dangerRadius);
                }
            }
        }
    }

    private void DamageNearby(float elapsed)
    {
        if (disaster == Disaster.Thunderstorm) return; // Lightning handles its own hits.
        foreach (CityStructure building in CityStructure.All)
        {
            if (building == null || building.IsDestroyed) continue;
            Vector3 offset = building.transform.position - location;
            offset.y = 0f;
            bool inArea = offset.sqrMagnitude <= dangerRadius * dangerRadius;
            if (!inArea && disaster == Disaster.Earthquake)
            {
                CityConstruction construction = FindAnyObjectByType<CityConstruction>();
                if (construction != null)
                {
                    Vector3 constructionOffset = building.transform.position - construction.BuildAreaPosition;
                    constructionOffset.y = 0f;
                    inArea = constructionOffset.sqrMagnitude <= dangerRadius * dangerRadius;
                }
            }
            if (!inArea) continue;
            if (disaster == Disaster.Tornado)
            {
                if (offset.sqrMagnitude > 25f) continue;
                building.DestroyObject();
            }
            else
                building.Damage((disaster == Disaster.Earthquake ? 8f : 2f) * elapsed * ExposureMultiplier);
            if (!damaged.Contains(building)) damaged.Add(building);
        }
    }

    private void DestroyResourcesAlongPath(Vector3 start, Vector3 end)
    {
        // Sweep the travelled path so fast movement cannot skip a tree between frames.
        foreach (Collider hit in Physics.OverlapCapsule(start + Vector3.up * 2f, end + Vector3.up * 2f, 5f, ~0, QueryTriggerInteraction.Ignore))
        {
            StaticResource resource = hit.GetComponentInParent<StaticResource>();
            if (resource != null && resource.IsActiveResource) resource.DestroyObject();
        }
    }

    private IEnumerator LightningStrikes()
    {
        for (int i = 0; i < 3; i++)
        {
            if (phase != Phase.Active || disaster != Disaster.Thunderstorm) yield break;
            LightningStrikeCount++;
            List<CityStructure> candidates = CityStructure.All.FindAll(building => building != null && !building.IsDestroyed && (i == 1 ? building.District == "Construction" : building.District != "Construction"));
            if (candidates.Count == 0) candidates = CityStructure.All.FindAll(building => building != null && !building.IsDestroyed);
            if (candidates.Count > 0)
            {
                CityStructure target = candidates[Random.Range(0, candidates.Count)];
                ShowLightning(target.transform.position);
                target.DestroyObject();
                if (!damaged.Contains(target)) damaged.Add(target);
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private void ShowLightning(Vector3 target)
    {
        GameObject bolt = new GameObject("Lightning strike");
        Light flash = bolt.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.color = new Color(0.75f, 0.85f, 1f);
        flash.range = 28f;
        flash.intensity = 8f;
        flash.shadows = LightShadows.None;
        bolt.transform.position = target + Vector3.up * 8f;
        LineRenderer line = bolt.AddComponent<LineRenderer>();
        line.positionCount = 7;
        line.startWidth = 0.8f;
        line.endWidth = 0.3f;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material boltMaterial = new Material(shader);
        boltMaterial.color = new Color(0.8f, 0.9f, 1f);
        line.material = boltMaterial;
        for (int i = 0; i < line.positionCount; i++)
        {
            float fraction = i / 6f;
            float sideways = i == 0 || i == 6 ? 0f : (i % 2 == 0 ? 2f : -2f);
            line.SetPosition(i, target + new Vector3(sideways, (1f - fraction) * 40f, 0));
        }
        Destroy(bolt, 0.5f);
        Destroy(boltMaterial, 0.6f);
    }

    private void FinishDisaster()
    {
        if (effect != null) Destroy(effect);
        if (constructionEffect != null) Destroy(constructionEffect);
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
        if (phase != Phase.Recovery || disaster != Disaster.Earthquake) return false;
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
        if (constructionEffect != null) Destroy(constructionEffect);
    }
}
