using UnityEngine;

// Small built-in effects with a fixed particle budget; no external art dependency.
public class CityDisasterVisual : MonoBehaviour
{
    private Transform water;
    private Material material;
    private float radius;
    private float age;

    public void Configure(CityDisasterEvent.Disaster type, float affectedRadius)
    {
        radius = affectedRadius;
        if (type == CityDisasterEvent.Disaster.Flood)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            surface.name = "Flood water";
            surface.transform.SetParent(transform, false);
            Destroy(surface.GetComponent<Collider>());
            water = surface.transform;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader);
            material.color = new Color(0.18f, 0.36f, 0.48f);
            surface.GetComponent<Renderer>().sharedMaterial = material;
            water.localScale = new Vector3(radius * 2, 0.1f, radius * 2);
            return;
        }

        GameObject emitter = new GameObject("Weather particles");
        emitter.transform.SetParent(transform, false);
        bool rain = type == CityDisasterEvent.Disaster.Thunderstorm;
        bool heat = type == CityDisasterEvent.Disaster.HeatWave;
        emitter.transform.localPosition = rain ? Vector3.up * 15f : Vector3.zero;
        emitter.transform.localRotation = Quaternion.Euler(rain ? 90f : -90f, 0, 0);
        ParticleSystem particles = emitter.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.maxParticles = rain ? 120 : 70;
        main.startLifetime = 2f;
        main.startSpeed = rain ? 12f : 3f;
        main.startSize = rain ? 0.12f : heat ? 0.5f : 2f;
        main.startColor = rain ? new Color(0.5f, 0.7f, 0.9f, 0.65f) : heat ? new Color(1f, 0.65f, 0.15f, 0.25f) : new Color(0.55f, 0.49f, 0.4f, 0.65f);
        var emission = particles.emission;
        emission.rateOverTime = rain ? 55f : 25f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = type == CityDisasterEvent.Disaster.Tornado ? 2f : radius;
        shape.angle = rain ? 0f : 15f;
        particles.Play();
    }

    private void Update()
    {
        if (water == null) return;
        age += Time.deltaTime;
        float height = Mathf.Lerp(0.1f, 0.8f, Mathf.Clamp01(age / 15f));
        water.localPosition = Vector3.up * height;
        water.localScale = new Vector3(radius * 2, height, radius * 2);
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }
}
