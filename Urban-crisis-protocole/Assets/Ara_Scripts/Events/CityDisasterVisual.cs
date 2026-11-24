using System.Collections.Generic;
using UnityEngine;

// Visuals only: the event controls damage, duration, and recovery.
public class CityDisasterVisual : MonoBehaviour
{
    private Transform funnel;
    private Mesh funnelMesh;
    private Material material;
    private float radius;
    private float age;
    private readonly Dictionary<Transform, Vector3> shakingBuildings = new Dictionary<Transform, Vector3>();

    public void Configure(CityDisasterEvent.Disaster type, float affectedRadius)
    {
        radius = affectedRadius;

        if (type == CityDisasterEvent.Disaster.Tornado)
        {
            CreateFunnel();
            return;
        }

        if (type == CityDisasterEvent.Disaster.Earthquake)
        {
            CreateEarthquake();
            return;
        }

        GameObject emitter = new GameObject("Weather particles");
        emitter.transform.SetParent(transform, false);
        bool rain = type == CityDisasterEvent.Disaster.Thunderstorm;
        emitter.transform.localPosition = rain ? Vector3.up * 15f : Vector3.zero;
        emitter.transform.localRotation = Quaternion.Euler(rain ? 90f : -90f, 0, 0);
        ParticleSystem particles = emitter.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.maxParticles = rain ? 160 : 100;
        main.startLifetime = 3f;
        main.startSpeed = rain ? 12f : 5f;
        main.startSize = rain ? 0.25f : 1.2f;
        main.startColor = rain ? new Color(0.5f, 0.7f, 0.9f, 0.65f) : new Color(1f, 0.65f, 0.15f, 0.25f);
        var emission = particles.emission;
        emission.rateOverTime = rain ? 70f : 35f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = radius;
        shape.angle = rain ? 0f : 15f;
        particles.Play();
    }

    private void CreateFunnel()
    {
        const int sides = 16;
        const int rings = 10;
        Vector3[] vertices = new Vector3[(rings + 1) * (sides + 1)];
        int[] triangles = new int[rings * sides * 6];
        for (int row = 0; row <= rings; row++)
        {
            float height = row / (float)rings;
            float width = Mathf.Lerp(0.6f, 7f, height);
            for (int side = 0; side <= sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides + height * 2f;
                vertices[row * (sides + 1) + side] = new Vector3(Mathf.Cos(angle) * width, height * 25f, Mathf.Sin(angle) * width);
            }
        }
        int index = 0;
        for (int row = 0; row < rings; row++)
            for (int side = 0; side < sides; side++)
            {
                int a = row * (sides + 1) + side;
                int b = a + sides + 1;
                triangles[index++] = a; triangles[index++] = b; triangles[index++] = a + 1;
                triangles[index++] = a + 1; triangles[index++] = b; triangles[index++] = b + 1;
            }
        GameObject objectRoot = new GameObject("Tornado funnel");
        objectRoot.transform.SetParent(transform, false);
        funnel = objectRoot.transform;
        funnelMesh = new Mesh { name = "Tornado funnel" };
        funnelMesh.vertices = vertices;
        funnelMesh.triangles = triangles;
        funnelMesh.RecalculateNormals();
        objectRoot.AddComponent<MeshFilter>().sharedMesh = funnelMesh;
        material = CreateMaterial(new Color(0.34f, 0.37f, 0.4f));
        objectRoot.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private void CreateEarthquake()
    {
        material = CreateMaterial(new Color(0.12f, 0.09f, 0.07f));
        for (int i = 0; i < 3; i++)
        {
            GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crack.name = "Earthquake ground crack";
            crack.transform.SetParent(transform, false);
            crack.transform.localPosition = new Vector3(i * 4f - 4f, 0.3f, 0f);
            crack.transform.localRotation = Quaternion.Euler(0, i * 32f - 32f, 0);
            crack.transform.localScale = new Vector3(0.45f, 0.15f, 16f);
            crack.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(crack.GetComponent<Collider>());
        }
        foreach (CityStructure building in CityStructure.All)
        {
            if (building == null || building.IsDestroyed) continue;
            Vector3 offset = building.transform.position - transform.position;
            offset.y = 0;
            if (offset.sqrMagnitude <= radius * radius)
                shakingBuildings[building.transform] = building.transform.position;
        }
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material result = new Material(shader);
        result.color = color;
        return result;
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (funnel != null) funnel.Rotate(Vector3.up, 150f * Time.deltaTime);
        foreach (var building in shakingBuildings)
            if (building.Key != null)
                building.Key.position = building.Value + new Vector3(Mathf.Sin(age * 28f) * 0.35f, 0, Mathf.Cos(age * 23f) * 0.25f);
    }

    private void OnDestroy()
    {
        foreach (var building in shakingBuildings)
            if (building.Key != null) building.Key.position = building.Value;
        if (funnelMesh != null) Destroy(funnelMesh);
        if (material != null) Destroy(material);
    }
}
