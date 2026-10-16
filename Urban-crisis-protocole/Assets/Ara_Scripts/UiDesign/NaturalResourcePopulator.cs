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

    private System.Random random;
    private List<Vector2> usedPositions = new List<Vector2>();
    private int resourceNumber;
    private int mountainNumber;

    private void OnEnable()
    {
        GenerateNature();
    }

    private void OnDisable()
    {
        ClearGeneratedObjects();
    }

    private void GenerateNature()
    {
        ClearGeneratedObjects();

        random = new System.Random(seed);
        usedPositions.Clear();
        resourceNumber = 0;
        mountainNumber = 0;

        GenerateResourcesAroundTown(humanTown);
        GenerateResourcesAroundTown(aiTown);
        GenerateResourcesOnOuterGround();
        GenerateMountains();
    }

    private void GenerateMountains()
    {
        float edgeX = mapSize.x / 2f - mountainEdgeInset;
        float edgeZ = mapSize.y / 2f - mountainEdgeInset;

        float horizontalSpacing = edgeX * 2f / mountainsPerLongSide;
        float verticalSpacing = edgeZ * 2f / mountainsPerShortSide;

        for (int i = 0; i < mountainsPerLongSide; i++)
        {
            float x = -edgeX + horizontalSpacing / 2f + i * horizontalSpacing;

            CreateMountain(x, edgeZ);
            CreateMountain(x, -edgeZ);
        }

        for (int i = 0; i < mountainsPerShortSide; i++)
        {
            float z = -edgeZ + verticalSpacing / 2f + i * verticalSpacing;

            CreateMountain(edgeX, z);
            CreateMountain(-edgeX, z);
        }
    }

    private void CreateMountain(float x, float z)
    {
        x += GetRandomNumber(-5f, 5f);
        z += GetRandomNumber(-5f, 5f);

        float height = mountainTemplate.transform.position.y;
        float angle = GetRandomNumber(0f, 360f);

        Vector3 position = new Vector3(x, height, z);
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

        GameObject mountain = Instantiate(
            mountainTemplate,
            position,
            rotation,
            mountainParent
        );

        float scale = GetRandomNumber(0.8f, 1.15f);

        mountainNumber++;
        mountain.transform.localScale = mountainTemplate.transform.localScale * scale;
        mountain.name = mountainTemplate.name + "_Generated_" + mountainNumber;

        if (!Application.isPlaying)
        {
            HidePreviewObject(mountain);
        }
    }

    private void GenerateResourcesAroundTown(Transform town)
    {
        Vector2 townCenter = new Vector2(town.position.x, town.position.z);

        float horizontalOffset = territorySize.x / 2f + resourceBandWidth / 2f;
        float verticalOffset = territorySize.y / 2f + resourceBandWidth / 2f;

        Vector2 sideSize = new Vector2(resourceBandWidth, territorySize.y);
        Vector2 topSize = new Vector2(
            territorySize.x + resourceBandWidth * 2f,
            resourceBandWidth
        );

        Vector2 rightSide = new Vector2(townCenter.x + horizontalOffset, townCenter.y);
        Vector2 leftSide = new Vector2(townCenter.x - horizontalOffset, townCenter.y);
        Vector2 topSide = new Vector2(townCenter.x, townCenter.y + verticalOffset);
        Vector2 bottomSide = new Vector2(townCenter.x, townCenter.y - verticalOffset);

        GenerateArea(rightSide, sideSize, treesPerSide, bushesPerSide, rocksPerSide);
        GenerateArea(leftSide, sideSize, treesPerSide, bushesPerSide, rocksPerSide);
        GenerateArea(topSide, topSize, treesPerSide, bushesPerSide, rocksPerSide);
        GenerateArea(bottomSide, topSize, treesPerSide, bushesPerSide, rocksPerSide);
    }

    private void GenerateResourcesOnOuterGround()
    {
        float sideWidth = (mapSize.x - existingMapSize.x) / 2f;
        float topHeight = (mapSize.y - existingMapSize.y) / 2f;

        Vector2 sideSize = new Vector2(sideWidth, existingMapSize.y);
        Vector2 topSize = new Vector2(mapSize.x, topHeight);

        float horizontalOffset = existingMapSize.x / 2f + sideWidth / 2f;
        float verticalOffset = existingMapSize.y / 2f + topHeight / 2f;

        GenerateOuterArea(new Vector2(horizontalOffset, 0f), sideSize);
        GenerateOuterArea(new Vector2(-horizontalOffset, 0f), sideSize);
        GenerateOuterArea(new Vector2(0f, verticalOffset), topSize);
        GenerateOuterArea(new Vector2(0f, -verticalOffset), topSize);
    }

    private void GenerateOuterArea(Vector2 center, Vector2 size)
    {
        GenerateArea(
            center,
            size,
            outerTreesPerArea,
            outerBushesPerArea,
            outerRocksPerArea
        );
    }

    private void GenerateArea(
        Vector2 center,
        Vector2 size,
        int treeAmount,
        int bushAmount,
        int rockAmount
    )
    {
        CreateResources(treeTemplate, ResourceType.Wood, treeAmount, center, size);
        CreateResources(bushTemplate, ResourceType.Wood, bushAmount, center, size);
        CreateResources(rockTemplate, ResourceType.Stone, rockAmount, center, size);
    }

    private void CreateResources(
        GameObject resourceTemplate,
        ResourceType resourceType,
        int amount,
        Vector2 center,
        Vector2 size
    )
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 mapPosition = FindFreePosition(center, size);
            float height = resourceTemplate.transform.position.y;
            float angle = GetRandomNumber(0f, 360f);

            Vector3 position = new Vector3(mapPosition.x, height, mapPosition.y);
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            GameObject resource = Instantiate(
                resourceTemplate,
                position,
                rotation,
                transform
            );

            float scale = GetRandomNumber(0.85f, 1.15f);

            resourceNumber++;
            resource.transform.localScale = resourceTemplate.transform.localScale * scale;
            resource.name = resourceTemplate.name + "_Generated_" + resourceNumber;

            if (Application.isPlaying)
            {
                MakeCollectible(resource, resourceType);
            }
            else
            {
                HidePreviewObject(resource);
            }

            usedPositions.Add(mapPosition);
        }
    }

    private Vector2 FindFreePosition(Vector2 center, Vector2 size)
    {
        float left = center.x - size.x / 2f;
        float right = center.x + size.x / 2f;
        float bottom = center.y - size.y / 2f;
        float top = center.y + size.y / 2f;

        Vector2 position = center;

        for (int attempt = 0; attempt < 24; attempt++)
        {
            float x = GetRandomNumber(left, right);
            float z = GetRandomNumber(bottom, top);

            position = new Vector2(x, z);

            if (IsPositionFree(position))
            {
                return position;
            }
        }

        return position;
    }

    private bool IsPositionFree(Vector2 position)
    {
        foreach (Vector2 usedPosition in usedPositions)
        {
            float distance = Vector2.Distance(position, usedPosition);

            if (distance < minimumSpacing)
            {
                return false;
            }
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

    private void ClearGeneratedObjects()
    {
        RemoveGeneratedChildren(transform);
        RemoveGeneratedChildren(mountainParent);
    }

    private void RemoveGeneratedChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (child.name.Contains("_Generated_"))
            {
                RemoveObject(child.gameObject);
            }
        }
    }

    private void RemoveObject(GameObject objectToRemove)
    {
        if (Application.isPlaying)
        {
            Destroy(objectToRemove);
        }
        else
        {
            DestroyImmediate(objectToRemove);
        }
    }

    private void HidePreviewObject(GameObject previewObject)
    {
        foreach (Transform child in previewObject.GetComponentsInChildren<Transform>())
        {
            child.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    private float GetRandomNumber(float minimum, float maximum)
    {
        float percentage = (float)random.NextDouble();
        return minimum + (maximum - minimum) * percentage;
    }
}
