using System.Collections.Generic;
using UnityEngine;

// A lightweight damage model for structures in the prototype neighborhood.
public class CityStructure : MonoBehaviour, IDestructible
{
    public static readonly List<CityStructure> All = new List<CityStructure>();

    [SerializeField] private float maximumHealth = 100f;
    public float Health { get; private set; }
    public bool IsDamaged => Health < maximumHealth;
    public float Condition => maximumHealth > 0f ? Health / maximumHealth : 0f;
    public string District { get; private set; }

    private Renderer buildingRenderer;
    private MaterialPropertyBlock properties;
    private Color originalColor;

    public void Configure(string district, Color color)
    {
        District = district;
        originalColor = color;
        buildingRenderer = GetComponent<Renderer>();
        properties = new MaterialPropertyBlock();
        Health = maximumHealth;
        UpdateAppearance();
    }

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    public void Damage(float amount)
    {
        Health = Mathf.Max(0f, Health - Mathf.Max(0f, amount));
        UpdateAppearance();
    }

    public void Repair()
    {
        Health = maximumHealth;
        UpdateAppearance();
    }

    public void DestroyObject() => Damage(maximumHealth);

    private void UpdateAppearance()
    {
        if (buildingRenderer == null) return;
        Color color = Color.Lerp(new Color(0.16f, 0.13f, 0.12f), originalColor, Condition);
        buildingRenderer.GetPropertyBlock(properties);
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        buildingRenderer.SetPropertyBlock(properties);
    }
}
