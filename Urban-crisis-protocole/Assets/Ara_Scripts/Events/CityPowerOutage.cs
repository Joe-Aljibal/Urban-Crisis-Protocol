using UnityEngine;

// A separate timer allows an outage and a fire to happen at the same time.
public class CityPowerOutage : MonoBehaviour
{
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float firstOutageDelay = 75f;
    [SerializeField] private float outageDuration = 45f;
    [SerializeField] private float cooldown = 150f;
    [SerializeField] private float repairCost = 100f;
    private float timer;
    public bool IsActive { get; private set; }
    public string Status { get; private set; } = "Power operating normally.";
    public float RepairCost => repairCost;

    private void Start() => timer = firstOutageDelay;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        if (IsActive) RestorePower();
        else TriggerOutage();
    }

    public void TriggerOutage()
    {
            if (IsActive) return;
            IsActive = true;
            timer = outageDuration;
            Status = "Blackout: neighborhood streetlights have lost power.";
            if (dayNightCycle != null) dayNightCycle.SetCityPower(false);
    }

    public bool DispatchRepair()
    {
        if (!IsActive || PlayerInfo.Instance.getMoney < repairCost) return false;
        PlayerInfo.Instance.addMoney(-repairCost);
        RestorePower();
        return true;
    }

    private void RestorePower()
    {
        IsActive = false;
        timer = cooldown;
        Status = "Power restored.";
        if (dayNightCycle != null) dayNightCycle.SetCityPower(true);
    }

    private void OnDestroy()
    {
        if (IsActive && dayNightCycle != null) dayNightCycle.SetCityPower(true);
    }
}
