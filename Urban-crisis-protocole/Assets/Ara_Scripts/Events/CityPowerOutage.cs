using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// A separate timer allows an outage and a fire to happen at the same time.
public class CityPowerOutage : MonoBehaviour
{
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float firstOutageDelay = 75f;
    [SerializeField] private float outageDuration = 10f;
    [SerializeField] private float cooldown = 150f;
    [SerializeField] private float repairCost = 100f;
    private float timer;
    private Coroutine flicker;
    private readonly Dictionary<Behaviour, bool> interruptedBuildings = new Dictionary<Behaviour, bool>();
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
            if (dayNightCycle != null) flicker = StartCoroutine(FlickerLights());
            foreach (CityStructure building in CityStructure.All)
                if (building != null && !building.IsDestroyed) IncludeBuilding(building.gameObject);
    }

    // Match the original event: lights flicker three times before the outage.
    private IEnumerator FlickerLights()
    {
        for (int i = 0; i < 3; i++)
        {
            dayNightCycle.SetCityPower(false);
            yield return new WaitForSeconds(0.15f);
            dayNightCycle.SetCityPower(true);
            yield return new WaitForSeconds(0.15f);
        }
        dayNightCycle.SetCityPower(false);
        flicker = null;
    }

    public void IncludeBuilding(GameObject building)
    {
        if (!IsActive || building == null) return;
        foreach (MonoBehaviour component in building.GetComponents<MonoBehaviour>())
        {
            if (!(component is IBuilding) || interruptedBuildings.ContainsKey(component)) continue;
            interruptedBuildings.Add(component, component.enabled);
            component.enabled = false;
        }
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
        if (flicker != null)
        {
            StopCoroutine(flicker);
            flicker = null;
        }
        IsActive = false;
        timer = cooldown;
        Status = "Power restored. Streetlights turn on at night.";
        foreach (var entry in interruptedBuildings)
        {
            if (entry.Key == null) continue;
            CityStructure condition = entry.Key.GetComponent<CityStructure>();
            if (condition == null || !condition.IsDestroyed) entry.Key.enabled = entry.Value;
        }
        interruptedBuildings.Clear();
        if (dayNightCycle != null) dayNightCycle.SetCityPower(true);
    }

    private void OnDestroy()
    {
        if (IsActive) RestorePower();
    }
}
