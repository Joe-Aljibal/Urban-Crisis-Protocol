using UnityEngine;

public class NaturalResourcePopulator : MonoBehaviour
{
    [SerializeField]
    private GameObject treeTemplate;

    [SerializeField]
    private GameObject bushTemplate;

    [SerializeField]
    private GameObject rockTemplate;

    [SerializeField]
    private Transform humanTown;

    [SerializeField]
    private Transform aiTown;

    [SerializeField]
    private Vector2 territorySize = new Vector2(36f, 52f);

    [SerializeField]
    private float resourceBandWidth = 14f;

    [SerializeField]
    private int treesPerSide = 7;

    [SerializeField]
    private int bushesPerSide = 4;

    [SerializeField]
    private int rocksPerSide = 1;

    [SerializeField]
    private int seed = 2026;

    private int resourceNumber;

    private void Awake()
    {
        System.Random random = new System.Random(seed);

        SpawnAroundTown(humanTown, random);
        SpawnAroundTown(aiTown, random);
    }

    private void SpawnAroundTown(Transform town, System.Random random)
    {
        Vector2 center = new Vector2(town.position.x, town.position.z);

        float xOffset = territorySize.x / 2f + resourceBandWidth / 2f;
        float zOffset = territorySize.y / 2f + resourceBandWidth / 2f;

        Vector2 sideBand = new Vector2(resourceBandWidth, territorySize.y);
        Vector2 topBottomBand = new Vector2(
            territorySize.x + resourceBandWidth * 2f,
            resourceBandWidth
        );

        SpawnArea(center + Vector2.right * xOffset, sideBand, random);
        SpawnArea(center + Vector2.left * xOffset, sideBand, random);
        SpawnArea(center + Vector2.up * zOffset, topBottomBand, random);
        SpawnArea(center + Vector2.down * zOffset, topBottomBand, random);
    }

    private void SpawnArea(Vector2 center, Vector2 size, System.Random random)
    {
        SpawnResources(
            treeTemplate,
            ResourceType.Wood,
            treesPerSide,
            center,
            size,
            random
        );

        SpawnResources(
            bushTemplate,
            ResourceType.Wood,
            bushesPerSide,
            center,
            size,
            random
        );

        SpawnResources(
            rockTemplate,
            ResourceType.Stone,
            rocksPerSide,
            center,
            size,
            random
        );
    }

    private void SpawnResources(
        GameObject resourceTemplate,
        ResourceType resourceType,
        int amount,
        Vector2 center,
        Vector2 size,
        System.Random random
    )
    {
        for (int i = 0; i < amount; i++)
        {
            float x = RandomRange(
                random,
                center.x - size.x / 2f,
                center.x + size.x / 2f
            );

            float z = RandomRange(
                random,
                center.y - size.y / 2f,
                center.y + size.y / 2f
            );

            Vector3 position = new Vector3(x, resourceTemplate.transform.position.y, z);
            Quaternion rotation = Quaternion.Euler(0f, RandomRange(random, 0f, 360f), 0f);

            GameObject resource = Instantiate(
                resourceTemplate,
                position,
                rotation,
                transform
            );

            float scale = RandomRange(random, 0.85f, 1.15f);

            resourceNumber++;
            resource.transform.localScale = resourceTemplate.transform.localScale * scale;
            resource.name = resourceTemplate.name + "_Generated_" + resourceNumber;

            MakeCollectible(resource, resourceType);
        }
    }

    private void MakeCollectible(GameObject resource, ResourceType resourceType)
    {
        foreach (MeshFilter mesh in resource.GetComponentsInChildren<MeshFilter>())
        {
            mesh.gameObject.AddComponent<BoxCollider>();
        }

        StaticResource staticResource = resource.AddComponent<StaticResource>();
        staticResource.Configure(resourceType, true);
    }

    private float RandomRange(System.Random random, float minimum, float maximum)
    {
        return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }
}
