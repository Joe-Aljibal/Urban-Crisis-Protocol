using System.Collections.Generic;
using UnityEngine;

// A lightweight damage model for structures in the prototype neighborhood.
public class CityStructure : MonoBehaviour, IDestructible
{
    public static readonly List<CityStructure> All = new List<CityStructure>();

    [SerializeField] private float maximumHealth = 100f;
    [SerializeField] private float health;
    public float Health => health;
    public bool IsDamaged => Health < maximumHealth;
    public float Condition => maximumHealth > 0f ? Health / maximumHealth : 0f;
    [SerializeField] private string district;
    public string District => district;

    [SerializeField] private Renderer buildingRenderer;
    [SerializeField] private Renderer[] buildingRenderers;
    [SerializeField] private bool[] rendererEnabled;
    [SerializeField] private Behaviour[] buildingBehaviours;
    [SerializeField] private bool[] behaviourEnabled;
    private MaterialPropertyBlock properties;
    [SerializeField] private Color originalColor;

    public void Configure(string district, Color color)
    {
        this.district = district;
        originalColor = color;
        buildingRenderers = GetComponentsInChildren<Renderer>(true);
        rendererEnabled = new bool[buildingRenderers.Length];
        for (int i = 0; i < buildingRenderers.Length; i++) rendererEnabled[i] = buildingRenderers[i].enabled;
        buildingRenderer = buildingRenderers.Length > 0 ? buildingRenderers[0] : null;
        buildingBehaviours = GetComponents<MonoBehaviour>();
        behaviourEnabled = new bool[buildingBehaviours.Length];
        for (int i = 0; i < buildingBehaviours.Length; i++) behaviourEnabled[i] = buildingBehaviours[i].enabled;
        properties = new MaterialPropertyBlock();
        health = maximumHealth;
        UpdateAppearance();
    }

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public void Damage(float amount)
    {
        if (destroyed) return;
        health = Mathf.Max(0f, health - Mathf.Max(0f, amount));
        if (health <= 0f) DestroyObject();
        else UpdateAppearance();
    }

    public void Repair()
    {
        health = maximumHealth;
        if (destroyed)
        {
            destroyed = false;
            if (buildingRenderers != null)
                for (int i = 0; i < buildingRenderers.Length; i++)
                    if (buildingRenderers[i] != null) buildingRenderers[i].enabled = rendererEnabled[i];
            if (buildingBehaviours != null)
                for (int i = 0; i < buildingBehaviours.Length; i++)
                    if (buildingBehaviours[i] != null && buildingBehaviours[i] != this) buildingBehaviours[i].enabled = behaviourEnabled[i];
            foreach (Collider part in GetComponentsInChildren<Collider>()) part.enabled = true;
        }
        UpdateAppearance();
    }

    [SerializeField] private bool destroyed;
    public bool IsDestroyed => destroyed;
    public void DestroyObject()
    {
        if (destroyed) return;
        destroyed = true;
        BlastDebris.Spawn(buildingRenderer);
        health = 0f;
        if (buildingRenderers != null)
            foreach (Renderer part in buildingRenderers) if (part != null) part.enabled = false;
        if (buildingBehaviours != null)
            foreach (Behaviour behaviour in buildingBehaviours)
                if (behaviour != null && behaviour != this) behaviour.enabled = false;
        foreach (Collider part in GetComponentsInChildren<Collider>()) part.enabled = false;
    }

    private void UpdateAppearance()
    {
        if (buildingRenderer == null) return;
        Color color = Color.Lerp(new Color(0.16f, 0.13f, 0.12f), originalColor, Condition);
        if (properties == null) properties = new MaterialPropertyBlock();
        buildingRenderer.GetPropertyBlock(properties);
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        buildingRenderer.SetPropertyBlock(properties);
    }
}
