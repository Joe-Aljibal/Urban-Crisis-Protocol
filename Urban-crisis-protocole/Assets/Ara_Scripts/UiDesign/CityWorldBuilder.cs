using System.Collections.Generic;
using UnityEngine;

// Builds a compact prototype from Unity primitives around an existing town center.
public class CityWorldBuilder : MonoBehaviour
{
    [SerializeField] private Transform townCenter;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField, Range(0, 12)] private int pedestrianCount = 6;

    private readonly List<Material> materials = new List<Material>();
    private Transform cityRoot;

    private void Start()
    {
        if (townCenter == null) return;
        cityRoot = new GameObject("Prototype neighborhood").transform;
        Vector3 center = townCenter.position;
        center.y = 0f;
        Material road = CreateMaterial(new Color(0.22f, 0.23f, 0.25f));
        Material pavement = CreateMaterial(new Color(0.60f, 0.59f, 0.56f));
        Material park = CreateMaterial(new Color(0.28f, 0.43f, 0.23f));
        Material lamp = CreateMaterial(new Color(0.25f, 0.26f, 0.27f));
        Material car = CreateMaterial(new Color(0.17f, 0.32f, 0.55f));
        Material person = CreateMaterial(new Color(0.8f, 0.58f, 0.3f));
        Material building = CreateMaterial(Color.white);

        // A loop avoids unsignalled crossings and provides an obvious vehicle route.
        Box("North road", center + new Vector3(0, 0.08f, 20), new Vector3(42, 0.1f, 4), road);
        Box("South road", center + new Vector3(0, 0.08f, -20), new Vector3(42, 0.1f, 4), road);
        Box("East road", center + new Vector3(19, 0.08f, 0), new Vector3(4, 0.1f, 40), road);
        Box("West road", center + new Vector3(-19, 0.08f, 0), new Vector3(4, 0.1f, 40), road);
        Box("North sidewalk", center + new Vector3(0, 0.12f, 16.8f), new Vector3(34, 0.14f, 2), pavement);
        Box("South sidewalk", center + new Vector3(0, 0.12f, -16.8f), new Vector3(34, 0.14f, 2), pavement);
        Box("East sidewalk", center + new Vector3(15.8f, 0.12f, 0), new Vector3(2, 0.14f, 32), pavement);
        Box("West sidewalk", center + new Vector3(-15.8f, 0.12f, 0), new Vector3(2, 0.14f, 32), pavement);
        Box("Central park", center + new Vector3(0, 0.1f, 0), new Vector3(8, 0.12f, 8), park);

        CreateBuilding(center + new Vector3(-9, 0, -10), "Residential", new Color(0.75f, 0.64f, 0.5f), 5f, building);
        CreateBuilding(center + new Vector3(0, 0, -10), "Residential", new Color(0.68f, 0.71f, 0.65f), 6f, building);
        CreateBuilding(center + new Vector3(9, 0, -10), "Residential", new Color(0.72f, 0.62f, 0.56f), 5f, building);
        CreateBuilding(center + new Vector3(-9, 0, 10), "Commercial", new Color(0.57f, 0.63f, 0.69f), 7f, building);
        CreateBuilding(center + new Vector3(0, 0, 10), "Emergency services", new Color(0.72f, 0.35f, 0.31f), 5f, building);
        CreateBuilding(center + new Vector3(9, 0, 10), "Industrial", new Color(0.59f, 0.58f, 0.53f), 6f, building);

        Vector3[] route = {
            center + new Vector3(-15.8f, 1, -16.8f), center + new Vector3(15.8f, 1, -16.8f),
            center + new Vector3(15.8f, 1, 16.8f), center + new Vector3(-15.8f, 1, 16.8f)
        };
        for (int i = 0; i < pedestrianCount; i++)
        {
            GameObject pedestrian = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pedestrian.name = "Citizen " + (i + 1);
            pedestrian.transform.SetParent(cityRoot, false);
            pedestrian.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            pedestrian.GetComponent<Renderer>().sharedMaterial = person;
            pedestrian.AddComponent<DestructibleObject>();
            pedestrian.AddComponent<CityPedestrian>().Configure(route, i % route.Length);
        }

        Vector3[] carRoute = {
            center + new Vector3(-19, 0.8f, -20), center + new Vector3(19, 0.8f, -20),
            center + new Vector3(19, 0.8f, 20), center + new Vector3(-19, 0.8f, 20)
        };
        GameObject vehicle = Box("City vehicle", carRoute[0], new Vector3(2f, 1.4f, 3.5f), car);
        vehicle.AddComponent<BoxCollider>();
        vehicle.AddComponent<DestructibleObject>();
        vehicle.AddComponent<CityTraffic>().Configure(carRoute);
        for (int i = 0; i < route.Length; i++)
        {
            Vector3 basePosition = route[i];
            basePosition.y = 0;
            Box("Streetlight pole", basePosition + Vector3.up * 2.5f, new Vector3(0.2f, 5f, 0.2f), lamp);
            GameObject lampObject = new GameObject("Streetlight");
            lampObject.transform.SetParent(cityRoot, false);
            lampObject.transform.position = basePosition + Vector3.up * 5f;
            Light light = lampObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = 2f;
            light.color = new Color(1f, 0.78f, 0.46f);
            light.shadows = LightShadows.None;
            if (dayNightCycle != null) dayNightCycle.RegisterNightLight(light);
        }
    }

    private void CreateBuilding(Vector3 position, string district, Color color, float height, Material material)
    {
        GameObject building = Box(district + " building", position + Vector3.up * (height * 0.5f), new Vector3(6f, height, 6f), material);
        building.AddComponent<CityStructure>().Configure(district, color);
        building.AddComponent<BoxCollider>();
    }

    private GameObject Box(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(cityRoot, false);
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(box.GetComponent<Collider>());
        return box;
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        materials.Add(material);
        return material;
    }

    private void OnDestroy()
    {
        if (cityRoot != null) Destroy(cityRoot.gameObject);
        foreach (Material material in materials)
            if (material != null) Destroy(material);
    }
}
