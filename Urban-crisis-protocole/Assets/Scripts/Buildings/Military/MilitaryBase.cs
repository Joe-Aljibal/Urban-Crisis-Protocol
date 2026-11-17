using System.Collections.Generic;
using UnityEngine;

public class MilitaryBase : MonoBehaviour, IBuilding
{
    [SerializeField] MilitaryBaseSO militaryBaseSO;
    PlayerInfo playerInfo = PlayerInfo.Instance;

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
        enabled = false; // Update (la solde) ne tourne que quand la base est active
    }

    void Start()
    {
        playerInfo.addWorkingPopulation(militaryBaseSO.population);
        playerInfo.electricityDependants.Add(this, 0);
        RequestElectricity();

        Log($"Placee : {militaryBaseSO.population} soldats, " +
            $"electricite {electricityReceived}/{militaryBaseSO.electricityNeeded}");
    }

    // Meme role que PlayerInfo.ElectrifyBuildings(), mais sur une copie de la liste :
    // une centrale vide se retire de electricityProviders pendant le parcours, ce qui
    // fait planter l'original ("Collection was modified").
    void RequestElectricity()
    {
        foreach (Electricity provider in new List<Electricity>(playerInfo.electricityProviders.Keys))
        {
            if (!playerInfo.electricityDependants.ContainsKey(this))
                break;

            provider.ElectrifyBuildings();
        }
    }

    void Update()
    {
        cooldown += Time.deltaTime;
        if (cooldown >= 1)
        {
            playerInfo.addMoney(-militaryBaseSO.upkeep);
            cooldown -= 1; // garde le reste pour ne pas perdre de temps entre deux paiements
        }
    }

    public void DeleteButton()
    {
        isDeleted = true;
        SetActive(false);
        playerInfo.addMoney(militaryBaseSO.price * .8f);
        playerInfo.UseElectricity(-electricityReceived);
        playerInfo.addWorkingPopulation(-militaryBaseSO.population);
        playerInfo.electricityDependants.Remove(this);

        Destroy(gameObject);
    }

    void OnDestroy() => activeBases.Remove(this);

    // Appele par Electricity quand la centrale qui alimente la base disparait.
    public void Deactivate(int electricityLost)
    {
        if (isDeleted)
            return;

        SetActive(false);
        electricityReceived -= electricityLost;
        playerInfo.UseElectricity(-electricityLost);

        Log($"Desactivee : electricite {electricityReceived}/{militaryBaseSO.electricityNeeded}");
    }

    // Appele par Electricity quand la base a recu toute son electricite.
    public void Activate()
    {
        SetActive(true);

        Log($"Active ! Bases actives : {activeBases.Count}, solde -{militaryBaseSO.upkeep} $/s");
    }

    void SetActive(bool active)
    {
        if (isActive == active)
            return;

        isActive = active;
        enabled = active;
        cooldown = 0;

        if (active)
        {
            if (!activeBases.Contains(this))
                activeBases.Add(this);
        }
        else
        {
            activeBases.Remove(this);
        }

        foreach (Solider soldier in soldiers)
            soldier.SetOnDuty(active);
    }

    public void UseElectricity(int electricity)
    {
        electricityReceived += electricity;
        playerInfo.UseElectricity(electricity);
    }

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
}
