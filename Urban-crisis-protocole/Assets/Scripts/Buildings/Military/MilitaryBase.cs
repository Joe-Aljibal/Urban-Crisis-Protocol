using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryBase : MonoBehaviour, IBuilding
{
    [SerializeField] MilitaryBaseSO militaryBaseSO;
    [Header("Voyant d'electricite")]
    [SerializeField] float indicatorHeight = 1f; // au-dessus du haut de la base
    [SerializeField] float indicatorSize = 0.4f;
    CityData playerInfo = Cities.cities["PLAYER"];

    static readonly Color poweredColor = new(0.2f, 0.9f, 0.3f);
    static readonly Color unpoweredColor = new(0.95f, 0.15f, 0.15f);
    static readonly Color unpoweredDimColor = new(0.35f, 0.05f, 0.05f);
    static readonly int baseColorId = Shader.PropertyToID("_BaseColor"); // URP Lit
    static readonly int colorId = Shader.PropertyToID("_Color");         // shaders Built-in

    Renderer indicator;
    MaterialPropertyBlock indicatorBlock;
    Coroutine blinkRoutine;

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
        CreatePowerIndicator();
        ShowPower(false);
    }

    // Petite sphere au-dessus de la base : verte si alimentee, rouge clignotante sinon.
    void CreatePowerIndicator()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "PowerIndicator";
        Destroy(sphere.GetComponent<Collider>()); // ne doit pas bloquer les clics sur la base

        sphere.transform.SetParent(transform, false);
        sphere.transform.position = new Vector3(bounds.center.x, bounds.max.y + indicatorHeight, bounds.center.z);

        // La base est aplatie (echelle non uniforme) : on compense pour garder une vraie sphere.
        Vector3 parentScale = transform.lossyScale;
        sphere.transform.localScale = new Vector3(
            indicatorSize / Mathf.Max(parentScale.x, 0.0001f),
            indicatorSize / Mathf.Max(parentScale.y, 0.0001f),
            indicatorSize / Mathf.Max(parentScale.z, 0.0001f));

        indicator = sphere.GetComponent<Renderer>();
        indicator.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        indicatorBlock = new MaterialPropertyBlock();
    }

    void ShowPower(bool powered)
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);
        blinkRoutine = null;

        if (powered) SetIndicatorColor(poweredColor);
        else blinkRoutine = StartCoroutine(BlinkUnpowered());
    }

    // Tourne meme si le script est desactive (enabled = false) : seule la solde s'arrete.
    IEnumerator BlinkUnpowered()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            SetIndicatorColor(unpoweredColor);
            yield return wait;
            SetIndicatorColor(unpoweredDimColor);
            yield return wait;
        }
    }

    void SetIndicatorColor(Color color)
    {
        indicatorBlock.SetColor(baseColorId, color);
        indicatorBlock.SetColor(colorId, color);
        indicator.SetPropertyBlock(indicatorBlock);
    }

    void Start()
    {
        playerInfo.addWorkingPopulation(militaryBaseSO.population);
        playerInfo.electricityDependants.Add(this);
        RequestElectricity();

        Log($"Placee : {militaryBaseSO.population} soldats, " +
            $"electricite {electricityReceived}/{militaryBaseSO.electricityNeeded}");
    }

    // Meme role que PlayerInfo.ElectrifyBuildings(), mais sur une copie de la liste :
    // une centrale vide se retire de electricityProviders pendant le parcours, ce qui
    // fait planter l'original ("Collection was modified").
    void RequestElectricity()
    {
        foreach (Electricity provider in new List<Electricity>(playerInfo.electricityProviders))
        {
            if (!playerInfo.electricityDependants.Contains(this))
                break;

            provider.ProvideElectricity();
        }
    }

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

        ShowPower(active);
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
    public void SetCityData(CityData data) => playerInfo = data ?? Cities.cities["PLAYER"];

    public void ReceiveElectricity(int amount)
    {
        electricityReceived += amount;
        if (HasEnoughElectricity()) Activate();
    }
    public void Deactivate(int electricityLost, int workersLost) => Deactivate(electricityLost);
    public void ReceiveWorkers(int workers) { }
    public int GetWorkersNeeded() => 0;
    public bool HasEnoughWorkers() => true;

}
