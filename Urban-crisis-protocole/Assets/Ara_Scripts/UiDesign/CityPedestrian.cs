using UnityEngine;
using System.Collections.Generic;

// Pedestrians move between fixed destinations; they don't need a NavMesh.
public class CityPedestrian : MonoBehaviour
{
    private enum State { Idle, Walking, Working, Fleeing, SeekingShelter, Injured, Returning }
    private static readonly List<CityPedestrian> citizens = new List<CityPedestrian>();
    public float Health { get; private set; } = 100f;
    public bool IsInjured => Health <= 35f;
    public static int InjuredCount => citizens.FindAll(citizen => citizen != null && citizen.IsInjured).Count;
    public static int CitizenCount => citizens.Count;

    private void OnEnable() { if (!citizens.Contains(this)) citizens.Add(this); }
    private void OnDisable() => citizens.Remove(this);

    public void TakeExposure(float amount) => Health = Mathf.Max(0f, Health - Mathf.Max(0f, amount));

    public static bool DispatchMedicalAid()
    {
        if (InjuredCount == 0 || PlayerInfo.Instance.getMoney < 100f) return false;
        if (!CityStructure.All.Exists(building => building != null && building.District == "Emergency services" && building.Condition > 0.5f)) return false;
        PlayerInfo.Instance.addMoney(-100f);
        foreach (CityPedestrian citizen in citizens)
        {
            if (citizen == null || !citizen.IsInjured) continue;
            citizen.Health = 100f;
            citizen.state = State.Returning;
            citizen.pause = 3f;
        }
        return true;
    }
    [SerializeField] private float walkingSpeed = 2.5f;
    [SerializeField] private float fleeingSpeed = 5f;
    private Vector3[] destinations;
    private int destinationIndex;
    private float pause;
    private State state;
    private Vector3 shelter;

    public void Configure(Vector3[] route, int startingIndex)
    {
        if (route == null || route.Length < 2) return;
        destinations = route;
        destinationIndex = startingIndex % route.Length;
        transform.position = route[destinationIndex];
        shelter = route[(destinationIndex + 2) % route.Length];
        destinationIndex = (destinationIndex + 1) % route.Length;
        state = State.Walking;
    }

    private void Update()
    {
        if (destinations == null || destinations.Length == 0) return;
        bool unsafeArea = CityHazards.TryGetDanger(transform.position, out Vector3 danger);
        if (unsafeArea)
        {
            CityDisasterEvent natural = CityDisasterEvent.Instance;
            float protection = natural != null && natural.TryGetDanger(transform.position, out _) ? natural.ExposureMultiplier : 1f;
            TakeExposure(8f * Time.deltaTime * protection);
            state = State.Fleeing;
            Vector3 away = transform.position - danger;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            MoveTowards(transform.position + away.normalized * 4f, IsInjured ? walkingSpeed : fleeingSpeed);
            return;
        }

        if (IsInjured) { state = State.Injured; return; }

        if (state == State.Fleeing)
        {
            // Choose a safe destination instead of running back into the same hazard.
            shelter = transform.position;
            float nearest = float.MaxValue;
            foreach (Vector3 destination in destinations)
            {
                if (CityHazards.TryGetDanger(destination, out _)) continue;
                float distance = (destination - transform.position).sqrMagnitude;
                if (distance < nearest) { nearest = distance; shelter = destination; }
            }
            state = State.SeekingShelter;
        }
        if (state == State.SeekingShelter)
        {
            if (MoveTowards(shelter, fleeingSpeed))
            {
                state = State.Returning;
                pause = 3f;
            }
            return;
        }
        if (pause > 0f)
        {
            pause -= Time.deltaTime;
            return;
        }
        Vector3 nextStep = Vector3.MoveTowards(transform.position, destinations[destinationIndex], 4f);
        if (CityHazards.TryGetDanger(nextStep, out _) || CityHazards.TryGetDanger(destinations[destinationIndex], out _))
        {
            state = State.Idle;
            return;
        }
        if (MoveTowards(destinations[destinationIndex], walkingSpeed))
        {
            state = destinationIndex % 2 == 0 ? State.Working : State.Idle;
            destinationIndex = (destinationIndex + 1) % destinations.Length;
            pause = Random.Range(1f, 3f);
        }
        else state = State.Walking;
    }

    private bool MoveTowards(Vector3 destination, float speed)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.04f) return true;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        return false;
    }
}
