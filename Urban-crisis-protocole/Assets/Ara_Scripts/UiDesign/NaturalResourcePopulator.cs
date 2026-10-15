using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NaturalResourcePopulator : MonoBehaviour
{
    [SerializeField]
    private GameObject treeTemplate;

    [SerializeField]
    private GameObject bushTemplate;

    [SerializeField]
    private GameObject rockTemplate;

    [SerializeField]
    private GameObject mountainTemplate;

    [SerializeField]
    private Transform humanTown;

    [SerializeField]
    private Transform aiTown;

    [SerializeField]
    private Transform mountainParent;

    [SerializeField]
    private Vector2 territorySize = new Vector2(60f, 75f);

    [SerializeField]
    private float resourceBandWidth = 20f;

    [SerializeField]
    private int treesPerSide = 12;

    [SerializeField]
    private int bushesPerSide = 6;

    [SerializeField]
    private int rocksPerSide = 3;

    [SerializeField]
    private Vector2 mapSize = new Vector2(540f, 350f);

    [SerializeField]
    private Vector2 existingMapSize = new Vector2(230f, 155f);

    [SerializeField]
    private int outerTreesPerArea = 50;

    [SerializeField]
    private int outerBushesPerArea = 25;

    [SerializeField]
    private int outerRocksPerArea = 10;

    [SerializeField]
    private int mountainsPerLongSide = 8;

    [SerializeField]
    private int mountainsPerShortSide = 5;

    [SerializeField]
    private float mountainEdgeInset = 20f;

    [SerializeField]
    private float minimumSpacing = 5f;

    [SerializeField]
    private int seed = 2026;

    private int resourceNumber;

    private int mountainNumber;

    private void OnEnable()
    {
        Populate();
    }

    private void OnDisable()
    {
        ClearGeneratedResources();
        ClearGeneratedMountains();
    }

    private void Populate()
    {
        ClearGeneratedResources();
        ClearGeneratedMountains();
        resourceNumber = 0;
        mountainNumber = 0;

        System.Random random = new System.Random(seed);
        List<Vector2> positions = new List<Vector2>();

        SpawnAroundTown(humanTown, positions, random);
        SpawnAroundTown(aiTown, positions, random);
        SpawnAcrossGround(positions, random);
        SpawnMountains(random);
    }

    private void SpawnMountains(System.Random random)
    {
        float edgeX = mapSize.x / 2f - mountainEdgeInset;
        float edgeZ = mapSize.y / 2f - mountainEdgeInset;

        for (int i = 0; i < mountainsPerLongSide; i++)
        {
            float x = Mathf.Lerp(
                -edgeX,
                edgeX,
                (i + 0.5f) / mountainsPerLongSide
            );

            SpawnMountain(new Vector2(x, edgeZ), random);
            SpawnMountain(new Vector2(x, -edgeZ), random);
        }

        for (int i = 0; i < mountainsPerShortSide; i++)
        {
            float z = Mathf.Lerp(
                -edgeZ,
                edgeZ,
                (i + 0.5f) / mountainsPerShortSide
            );

            SpawnMountain(new Vector2(edgeX, z), random);
            SpawnMountain(new Vector2(-edgeX, z), random);
        }
    }

    private void SpawnMountain(Vector2 mapPosition, System.Random random)
    {
        mapPosition += new Vector2(
            RandomRange(random, -5f, 5f),
            RandomRange(random, -5f, 5f)
        );

        Vector3 position = new Vector3(
            mapPosition.x,
            mountainTemplate.transform.position.y,
            mapPosition.y
        );
        Quaternion rotation = Quaternion.Euler(0f, RandomRange(random, 0f, 360f), 0f);

        GameObject mountain = Instantiate(
            mountainTemplate,
            position,
            rotation,
            mountainParent
        );

        float scale = RandomRange(random, 0.8f, 1.15f);

        mountainNumber++;
        mountain.transform.localScale = mountainTemplate.transform.localScale * scale;
        mountain.name = mountainTemplate.name + "_Generated_" + mountainNumber;

        if (!Application.isPlaying)
        {
            MakeEditorPreview(mountain);
        }
    }

    private void SpawnAroundTown(
        Transform town,
        List<Vector2> positions,
        System.Random random
    )
    {
        Vector2 center = new Vector2(town.position.x, town.position.z);

        float xOffset = territorySize.x / 2f + resourceBandWidth / 2f;
        float zOffset = territorySize.y / 2f + resourceBandWidth / 2f;

        Vector2 sideBand = new Vector2(resourceBandWidth, territorySize.y);
        Vector2 topBottomBand = new Vector2(
            territorySize.x + resourceBandWidth * 2f,
            resourceBandWidth
        );

        SpawnArea(
            center + Vector2.right * xOffset,
            sideBand,
            treesPerSide,
            bushesPerSide,
            rocksPerSide,
            positions,
            random
        );

        SpawnArea(
            center + Vector2.left * xOffset,
            sideBand,
            treesPerSide,
            bushesPerSide,
            rocksPerSide,
            positions,
            random
        );

        SpawnArea(
            center + Vector2.up * zOffset,
            topBottomBand,
            treesPerSide,
            bushesPerSide,
            rocksPerSide,
            positions,
            random
        );

        SpawnArea(
            center + Vector2.down * zOffset,
            topBottomBand,
            treesPerSide,
            bushesPerSide,
            rocksPerSide,
            positions,
            random
        );
    }

    private void SpawnAcrossGround(List<Vector2> positions, System.Random random)
    {
        float sideWidth = (mapSize.x - existingMapSize.x) / 2f;
        float topBottomHeight = (mapSize.y - existingMapSize.y) / 2f;

        Vector2 sideArea = new Vector2(sideWidth, existingMapSize.y);
        Vector2 topBottomArea = new Vector2(mapSize.x, topBottomHeight);

        float xOffset = existingMapSize.x / 2f + sideWidth / 2f;
        float zOffset = existingMapSize.y / 2f + topBottomHeight / 2f;

        SpawnOuterArea(Vector2.right * xOffset, sideArea, positions, random);
        SpawnOuterArea(Vector2.left * xOffset, sideArea, positions, random);
        SpawnOuterArea(Vector2.up * zOffset, topBottomArea, positions, random);
        SpawnOuterArea(Vector2.down * zOffset, topBottomArea, positions, random);
    }

    private void SpawnOuterArea(
        Vector2 center,
        Vector2 size,
        List<Vector2> positions,
        System.Random random
    )
    {
        SpawnArea(
            center,
            size,
            outerTreesPerArea,
            outerBushesPerArea,
            outerRocksPerArea,
            positions,
            random
        );
    }

    private void SpawnArea(
        Vector2 center,
        Vector2 size,
        int treeAmount,
        int bushAmount,
        int rockAmount,
        List<Vector2> positions,
        System.Random random
    )
    {
        SpawnResources(
            treeTemplate,
            ResourceType.Wood,
            treeAmount,
            center,
            size,
            positions,
            random
        );

        SpawnResources(
            bushTemplate,
            ResourceType.Wood,
            bushAmount,
            center,
            size,
            positions,
            random
        );

        SpawnResources(
            rockTemplate,
            ResourceType.Stone,
            rockAmount,
            center,
            size,
            positions,
            random
        );
    }

    private void SpawnResources(
        GameObject resourceTemplate,
        ResourceType resourceType,
        int amount,
        Vector2 center,
        Vector2 size,
        List<Vector2> positions,
        System.Random random
    )
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 mapPosition = FindPosition(center, size, positions, random);
            Vector3 position = new Vector3(
                mapPosition.x,
                resourceTemplate.transform.position.y,
                mapPosition.y
            );
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

            if (Application.isPlaying)
            {
                MakeCollectible(resource, resourceType);
            }
            else
            {
                MakeEditorPreview(resource);
            }

            positions.Add(mapPosition);
        }
    }

    private void ClearGeneratedResources()
    {
        ClearGeneratedChildren(transform);
    }

    private void ClearGeneratedMountains()
    {
        ClearGeneratedChildren(mountainParent);
    }

    private void ClearGeneratedChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (!child.name.Contains("_Generated_"))
                continue;

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void MakeEditorPreview(GameObject resource)
    {
        HideFlags previewFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;

        foreach (Transform child in resource.GetComponentsInChildren<Transform>())
        {
            child.gameObject.hideFlags = previewFlags;
        }
    }

    private Vector2 FindPosition(
        Vector2 center,
        Vector2 size,
        List<Vector2> positions,
        System.Random random
    )
    {
        Vector2 position = center;

        for (int attempt = 0; attempt < 24; attempt++)
        {
            position = new Vector2(
                RandomRange(random, center.x - size.x / 2f, center.x + size.x / 2f),
                RandomRange(random, center.y - size.y / 2f, center.y + size.y / 2f)
            );

            if (IsFarEnough(position, positions))
                break;
        }

        return position;
    }

    private bool IsFarEnough(Vector2 position, List<Vector2> positions)
    {
        foreach (Vector2 otherPosition in positions)
        {
            if (Vector2.Distance(position, otherPosition) < minimumSpacing)
                return false;
        }

        return true;
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
