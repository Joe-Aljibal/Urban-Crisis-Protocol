using System.Collections.Generic;
using UnityEngine;

public class MilitaryBase : MonoBehaviour, IBuilding, ISellable
{
    [SerializeField] MilitaryBaseSO militaryBaseSO;
    CityData playerInfo = Cities.cities["PLAYER"];

    // Toutes les bases actives de la ville (utilise par les evenements de crise).
    static readonly List<MilitaryBase> activeBases = new();

    Solider[] soldiers;
    int electricityReceived = 0;
    float cooldown = 0;
    bool isActive = false; // la base ne defend qu'une fois alimentee
    bool isDeleted = false;

    // Vide la liste si le domain reload est desactive (Enter Play Mode Options).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => activeBases.Clear();

    void Awake()
    {
        soldiers = GetComponentsInChildren<Solider>(true);
        // Start must register the base before it can receive power.
    }

    void Start()
    {
        playerInfo.RegisterBuilding(this);

        Log($"Placee : {militaryBaseSO.population} soldats, " +
            $"electricite {electricityReceived}/{militaryBaseSO.electricityNeeded}");
    }

    // Meme role que PlayerInfo.ElectrifyBuildings(), mais sur une copie de la liste :
    // une centrale vide se retire de electricityProviders pendant le parcours, ce qui
    // fait planter l'original ("Collection was modified").
    void Update()
    {
        if (!isActive) return;
        cooldown += Time.deltaTime;
        if (cooldown >= 1)
        {
            playerInfo.addMoney(-militaryBaseSO.upkeep);
            cooldown -= 1; // garde le reste pour ne pas perdre de temps entre deux paiements
        }
    }

    public int PowerDemand => militaryBaseSO.electricityNeeded;
    public int WorkerDemand => militaryBaseSO.population;
    private int workersReceived;
    public void ApplySupply(int power, int workers, bool available)
    {
        electricityReceived = power;
        workersReceived = workers;
        SetActive(available && HasEnoughElectricity() && HasEnoughWorkers());
    }
    public void DeleteButton()
    {
        if (isDeleted) return;
        isDeleted = true;
        SetActive(false);
        playerInfo.UnregisterBuilding(this);
        playerInfo.RemoveBuilding(gameObject);
        playerInfo.addMoney(militaryBaseSO.price * .8f);
        Destroy(gameObject);
    }
    public void Sell() => DeleteButton();
    void OnDestroy()
    {
        activeBases.Remove(this);
        if (!isDeleted) { playerInfo.UnregisterBuilding(this); playerInfo.RemoveBuilding(gameObject); }
    }
    public void Deactivate(int electricityLost) => playerInfo.SetBuildingAvailable(this, false);
    public void Activate() => playerInfo.SetBuildingAvailable(this, true);
    void SetActive(bool active)
    {
        if (isActive == active) return;
        isActive = active;
        cooldown = 0;
        if (active) { if (!activeBases.Contains(this)) activeBases.Add(this); }
        else activeBases.Remove(this);
        foreach (Solider soldier in soldiers) if (soldier != null) soldier.SetOnDuty(active);
    }
    public void UseElectricity(int electricity) => playerInfo.RecalculateBuildings();

    // Chance combinee de toutes les bases actives : 1 - produit(1 - p).
    public static bool TryInterceptBandit()
    {
        if (activeBases.Count == 0)
            return false;

        float escapeChance = 1;
        foreach (MilitaryBase militaryBase in activeBases)
            escapeChance *= 1 - militaryBase.militaryBaseSO.interceptChance;

        return Random.value > escapeChance;
    }

    // Les logs ne sont gardes que dans l'editeur (rien dans le build final).
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    static void Log(string message) => Debug.Log("[Base militaire] " + message);

    public static int ActiveBaseCount => activeBases.Count;

    public bool IsActive() => isActive;
    public bool HasEnoughElectricity() => electricityReceived >= militaryBaseSO.electricityNeeded;
    public IBuildingSO GetBuildingSO() => militaryBaseSO;
    public string GetBuildingType() => militaryBaseSO.type;
    public string GetRessource() => militaryBaseSO.ressource;
    public int GetPopulation() => militaryBaseSO.population;
    public int GetPrice() => militaryBaseSO.price;
    public int GetElectricityReceived() => electricityReceived;
    public int GetElectricityNeeded() => militaryBaseSO.electricityNeeded;
    public bool CanPlace() => militaryBaseSO.CanPlace();
    public void SetCityData(CityData data) => playerInfo = data ?? Cities.cities["PLAYER"];

    public void ReceiveElectricity(int amount) => playerInfo.RecalculateBuildings();
    public void Deactivate(int electricityLost, int workersLost) => playerInfo.RecalculateBuildings();
    public void ReceiveWorkers(int workers) => playerInfo.RecalculateBuildings();
    public int GetWorkersNeeded() => Mathf.Max(0, WorkerDemand - workersReceived);
    public bool HasEnoughWorkers() => workersReceived >= WorkerDemand;
}
