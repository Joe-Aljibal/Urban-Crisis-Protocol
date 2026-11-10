using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// A small, self-contained crisis for the existing world scene.
public class CityFireEvent : MonoBehaviour
{
    public static CityFireEvent Instance { get; private set; }
    private bool panelOpen;
    public static bool IsMenuOpen => Instance != null && Instance.panelOpen;
    private static Rect PanelRect => new Rect(20f, Mathf.Min(ResourceUIManager.PanelBottom + 8f, Screen.height - 120f), Mathf.Min(440f, Screen.width - 40f), Mathf.Max(110f, Mathf.Min(450f, Screen.height - ResourceUIManager.PanelBottom - 28f)));
    private static Rect TabRect => new Rect(20f, Mathf.Min(ResourceUIManager.PanelBottom + 56f, Screen.height - 48f), 205f, 40f);
    public static void ClosePanel() { if (Instance != null) Instance.panelOpen = false; }
    public static bool IsPointerOverPanel
    {
        get
        {
            if (Instance == null || CityConstruction.IsMenuOpen || Mouse.current == null) return false;
            Vector2 mouse = Mouse.current.position.ReadValue();
            return (Instance.panelOpen ? PanelRect : TabRect)
                .Contains(new Vector2(mouse.x, Screen.height - mouse.y));
        }
    }

    private enum Stage { Waiting, Warning, Active, Recovery }

    [SerializeField] private float firstEventDelay = 30f;
    [SerializeField] private float cooldown = 90f;
    [SerializeField] private float warningDuration = 8f;
    [SerializeField] private float fireDuration = 30f;
    [SerializeField] private float dangerRadius = 12f;
    [SerializeField] private float responseCost = 100f;
    [SerializeField] private float repairCost = 50f;
    [SerializeField] private float spreadDelay = 10f;
    [SerializeField] private float spreadRadius = 11f;

    private Stage stage;
    private float timer;
    private StaticResource target;
    private CityStructure affectedBuilding;
    private readonly List<CityStructure> damagedBuildings = new List<CityStructure>();
    private float damageTimer;
    private bool hasSpread;
    private Vector3 location;
    private GameObject fireVisual;
    private GameObject otherFireVisual;
    private GameObject damageVisual;
    private TownCameraController townCamera;
    private CityPowerOutage powerOutage;
    private CityDisasterEvent naturalDisaster;
    private Vector2 panelScroll;
    private bool showEventButtons;
    private GUISkin largeSkin;
    private string message = "No active emergencies.";

    private void Awake()
    {
        Instance = this;
        timer = firstEventDelay;
    }

    private void Start()
    {
        townCamera = FindFirstObjectByType<TownCameraController>();
        powerOutage = GetComponent<CityPowerOutage>();
        naturalDisaster = GetComponent<CityDisasterEvent>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) TogglePanel();
        if (stage == Stage.Active && affectedBuilding != null)
            UpdateBuildingDamage();
        timer -= Time.deltaTime;
        if (timer > 0f || stage == Stage.Recovery)
            return;

        switch (stage)
        {
            case Stage.Waiting:
                StartWarning();
                break;
            case Stage.Warning:
                StartFire();
                break;
            case Stage.Active:
                FinishFire(false);
                break;
        }
    }

    public void StartFireEvent()
    {
        if (stage == Stage.Waiting) StartWarning();
    }

    private void StartWarning()
    {
        affectedBuilding = null;
        damagedBuildings.Clear();
        List<CityStructure> availableBuildings = CityStructure.All.FindAll(building => building != null && !building.IsDestroyed);
        if (availableBuildings.Count > 0)
        {
            affectedBuilding = availableBuildings[Random.Range(0, availableBuildings.Count)];
            if (affectedBuilding != null)
            {
                location = affectedBuilding.transform.position;
                stage = Stage.Warning;
                timer = warningDuration;
                message = "Fire warning in " + affectedBuilding.District + ". Dispatch a response.";
                return;
            }
        }
        Transform[] trees = ResourceManager.GetResourceTransforms(ResourceType.Wood);
        int start = trees.Length > 0 ? Random.Range(0, trees.Length) : 0;
        target = null;
        for (int i = 0; i < trees.Length; i++)
        {
            Transform tree = trees[(start + i) % trees.Length];
            if (tree == null) continue;
            StaticResource candidate = tree.GetComponent<StaticResource>();
            if (candidate != null && candidate.IsActiveResource && candidate.IsAvailable)
            {
                target = candidate;
                break;
            }
        }

        if (target == null)
        {
            timer = cooldown;
            message = "No available trees for a fire. Retrying later.";
            return;
        }

        location = target.transform.position;
        stage = Stage.Warning;
        timer = warningDuration;
        message = "Fire warning: dispatch a response before it spreads.";
    }

    private void StartFire()
    {
        if (affectedBuilding == null && (target == null || !target.IsActiveResource))
        {
            ResetEvent("The fire risk passed.");
            return;
        }

        stage = Stage.Active;
        timer = fireDuration;
        damageTimer = 0f;
        hasSpread = false;
        if (affectedBuilding != null) damagedBuildings.Add(affectedBuilding);
        message = "Fire active: nearby collectors are evacuating.";
        fireVisual = new GameObject("Fire effect");
        fireVisual.transform.position = location;
        fireVisual.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        ParticleSystem particles = fireVisual.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 2f;
        main.startSpeed = 3f;
        main.startSize = 3f;
        main.startColor = new Color(1f, 0.35f, 0.04f, 0.8f);
        main.maxParticles = 90;
        var emission = particles.emission;
        emission.rateOverTime = 40f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 1.5f;
        particles.Play();
        if (affectedBuilding != null)
        {
            bool constructionFire = affectedBuilding.District == "Construction";
            CityStructure other = CityStructure.All.Find(building => building != null && !building.IsDestroyed && building != affectedBuilding && (building.District == "Construction") != constructionFire);
            if (other != null)
            {
                damagedBuildings.Add(other);
                otherFireVisual = Instantiate(fireVisual, other.transform.position, fireVisual.transform.rotation);
            }
        }
    }

    private void UpdateBuildingDamage()
    {
        if (!hasSpread && timer <= fireDuration - spreadDelay)
        {
            hasSpread = true;
            foreach (CityStructure building in CityStructure.All)
            {
                if (building != null && building != affectedBuilding &&
                    (building.transform.position - location).sqrMagnitude <= spreadRadius * spreadRadius)
                    damagedBuildings.Add(building);
            }
            if (damagedBuildings.Count > 1)
                message = "Fire is spreading to nearby buildings!";
        }
        damageTimer += Time.deltaTime;
        if (damageTimer < 1f) return;
        float elapsed = damageTimer;
        damageTimer = 0f;
        foreach (CityStructure building in damagedBuildings)
            if (building != null) building.Damage(2.5f * elapsed);
    }

    private void FinishFire(bool contained)
    {
        if (fireVisual != null) Destroy(fireVisual);
        if (otherFireVisual != null) Destroy(otherFireVisual);
        if (affectedBuilding != null)
        {
            if (damagedBuildings.Exists(building => building != null && building.IsDamaged))
            {
                stage = Stage.Recovery;
                message = "Fire contained. The building needs repairs.";
            }
            else ResetEvent("Fire prevented. The area is safe.");
            return;
        }
        if (contained)
        {
            ResetEvent("Fire contained. The area is safe.");
            return;
        }

        if (target != null && target.IsActiveResource)
        {
            target.Disable();
            damageVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            damageVisual.name = "Burned tree";
            damageVisual.transform.position = location + Vector3.up * 0.5f;
            damageVisual.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            Destroy(damageVisual.GetComponent<Collider>());
            damageVisual.GetComponent<Renderer>().material.color = new Color(0.18f, 0.12f, 0.1f);
            stage = Stage.Recovery;
            message = "Fire burned a resource. Repair the area to restore it.";
        }
        else ResetEvent("The fire ended; the affected resource was already gone.");
    }

    private void ResetEvent(string result)
    {
        stage = Stage.Waiting;
        timer = cooldown;
        target = null;
        affectedBuilding = null;
        damagedBuildings.Clear();
        message = result;
    }

    public bool RepairFireDamage()
    {
        if (stage != Stage.Recovery || PlayerInfo.Instance.getMoney < repairCost) return false;
        foreach (CityStructure building in damagedBuildings)
            if (building != null) building.Repair();
        if (target != null) target.Restore();
        if (damageVisual != null) Destroy(damageVisual);
        PlayerInfo.Instance.addMoney(-repairCost);
        ResetEvent("The damaged area has been restored.");
        return true;
    }

    public bool RebuildDestroyedBuildings()
    {
        if (!CityStructure.All.Exists(building => building != null && building.IsDestroyed) || PlayerInfo.Instance.getMoney < 150f)
            return false;
        foreach (CityStructure building in CityStructure.All)
            if (building != null && building.IsDestroyed) building.Repair();
        PlayerInfo.Instance.addMoney(-150f);
        message = "Destroyed buildings rebuilt.";
        return true;
    }

    public bool TryGetDanger(Vector3 position, out Vector3 dangerCenter)
    {
        dangerCenter = location;
        return stage == Stage.Active && (position - location).sqrMagnitude < dangerRadius * dangerRadius;
    }

    private void TogglePanel()
    {
        panelOpen = !panelOpen;
        if (panelOpen && CityConstruction.IsMenuOpen)
            FindAnyObjectByType<CityConstruction>()?.ToggleMenu();
    }

    private void OnGUI()
    {
        if (CityConstruction.IsMenuOpen) return;
        if (!panelOpen)
        {
            if (GUI.Button(TabRect, "Events (F)")) TogglePanel();
            return;
        }
        GUISkin previousSkin = GUI.skin;
        if (largeSkin == null)
        {
            largeSkin = Instantiate(GUI.skin);
            largeSkin.label.fontSize = 20;
            largeSkin.label.wordWrap = true;
            largeSkin.button.fontSize = 19;
            largeSkin.button.fixedHeight = 42;
            largeSkin.button.wordWrap = true;
        }
        GUI.skin = largeSkin;
        GUILayout.BeginArea(PanelRect, GUI.skin.box);
        panelScroll = GUILayout.BeginScrollView(panelScroll);
        GUILayout.BeginHorizontal();
        GUILayout.Label("CITY EMERGENCIES");
        if (GUILayout.Button("Close (F)", GUILayout.Width(130f))) TogglePanel();
        GUILayout.EndHorizontal();
        GUILayout.Label("Citizens: " + CityPedestrian.CitizenCount + " | Injured: " + CityPedestrian.InjuredCount);
        if (CityPedestrian.InjuredCount > 0 && GUILayout.Button("Dispatch medical aid ($100)"))
            if (!CityPedestrian.DispatchMedicalAid()) message = "Medical aid needs $100 and an operational emergency station.";
        GUILayout.Label(message);
        if (stage != Stage.Waiting)
        {
            GUILayout.Label("Location: " + location.x.ToString("F0") + ", " + location.z.ToString("F0"));
            GUILayout.Label("Severity: " + (stage == Stage.Active ? "High" : stage == Stage.Warning ? "Warning" : "Recovery"));
            if (affectedBuilding != null && !affectedBuilding.IsDestroyed)
            {
                GUILayout.Label("Building condition: " + Mathf.RoundToInt(affectedBuilding.Condition * 100f) + "%");
                if (damagedBuildings.Count > 1) GUILayout.Label("Buildings affected: " + damagedBuildings.Count);
            }
            if (townCamera != null && GUILayout.Button("Focus on location")) townCamera.FocusOnGround(location);
        }
        if ((stage == Stage.Warning || stage == Stage.Active) && GUILayout.Button("Dispatch fire response ($" + responseCost + ")"))
        {
            if (PlayerInfo.Instance.getMoney >= responseCost)
            {
                PlayerInfo.Instance.addMoney(-responseCost);
                FinishFire(true);
            }
            else message = "Not enough money to dispatch a response.";
        }
        if (stage == Stage.Recovery && GUILayout.Button("Restore area ($" + repairCost + ")"))
        {
            if (!RepairFireDamage()) message = "Not enough money to restore this area.";
        }
        if (powerOutage != null && powerOutage.IsActive)
        {
            GUILayout.Space(4f);
            GUILayout.Label(powerOutage.Status);
            if (powerOutage.IsActive && GUILayout.Button("Repair power ($" + powerOutage.RepairCost + ")"))
            {
                if (!powerOutage.DispatchRepair()) message = "Not enough money to repair power.";
            }
        }
        if (naturalDisaster != null && naturalDisaster.HasIncident)
        {
            GUILayout.Space(4f);
            GUILayout.Label(naturalDisaster.Status);
            if (naturalDisaster.CanRespond && GUILayout.Button("Prepare emergency response ($100)"))
                if (!naturalDisaster.DispatchResponse()) message = "Not enough money to prepare the area.";
            if (naturalDisaster.HasIncident && GUILayout.Button("Focus on natural disaster")) naturalDisaster.Focus();
            if (naturalDisaster.NeedsRepair && GUILayout.Button("Repair disaster damage ($" + naturalDisaster.RepairCost + ")"))
            {
                if (!naturalDisaster.Repair()) message = "Not enough money to repair disaster damage.";
            }
        }
        CityLegacyEvents legacy = CityLegacyEvents.Instance;
        if (legacy != null && legacy.IsReady && legacy.RaidRunning)
        {
            GUILayout.Label(legacy.Status);
            if (GUILayout.Button("Dispatch security ($100)") && !legacy.DispatchSecurity())
                message = "Not enough money to dispatch security.";
        }

        if (GUILayout.Button(showEventButtons ? "Hide event controls" : "Start an event"))
            showEventButtons = !showEventButtons;

        if (showEventButtons)
        {
            if (stage == Stage.Waiting && GUILayout.Button("Fire")) StartFireEvent();
            if (powerOutage != null && !powerOutage.IsActive && GUILayout.Button("Power outage"))
                powerOutage.TriggerOutage();
            if (legacy != null && legacy.IsReady)
            {
                if (!legacy.RaidRunning && GUILayout.Button("Bandit raid")) legacy.StartRaid();
                if (GUILayout.Button("Nuclear event")) legacy.LaunchBomb();
            }

            CityStructure eventTarget = CityStructure.All.Find(building => building != null && !building.IsDestroyed);
            if (naturalDisaster != null && !naturalDisaster.HasIncident && eventTarget != null)
            {
                foreach (CityDisasterEvent.Disaster type in System.Enum.GetValues(typeof(CityDisasterEvent.Disaster)))
                {
                    string label = type.ToString();
                    if (GUILayout.Button(label) && naturalDisaster.TryStart(type, eventTarget.transform.position))
                        naturalDisaster.Focus();
                }
            }
        }

        if (CityStructure.All.Exists(building => building != null && building.IsDestroyed))
        {
            if (GUILayout.Button("Rebuild destroyed buildings ($150)"))
            {
                if (!RebuildDestroyedBuildings()) message = "Not enough money to rebuild.";
            }
        }


        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUI.skin = previousSkin;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (largeSkin != null) Destroy(largeSkin);
        if (fireVisual != null) Destroy(fireVisual);
        if (otherFireVisual != null) Destroy(otherFireVisual);
        if (damageVisual != null) Destroy(damageVisual);
    }
}
