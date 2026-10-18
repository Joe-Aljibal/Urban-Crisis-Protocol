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
    private float resourceBandWidth = 40f;

    [SerializeField]
    private int treesPerSide = 5;

    [SerializeField]
    private int bushesPerSide = 2;

    [SerializeField]
    private int rocksPerSide = 1;

    [SerializeField]
    private Vector2 mapSize = new Vector2(540f, 350f);

    [SerializeField]
    private Vector2 existingMapSize = new Vector2(230f, 155f);

    [SerializeField]
    private int outerTreesPerArea = 72;

    [SerializeField]
    private int outerBushesPerArea = 36;

    [SerializeField]
    private int outerRocksPerArea = 14;

    [SerializeField]
    private int mountainsPerLongSide = 8;

    [SerializeField]
    private int mountainsPerShortSide = 5;

    [SerializeField]
    private float mountainEdgeInset = 20f;

    [SerializeField]
    private float mountainBandDepth = 35f;

    [SerializeField]
    private float riverMountainClearance = 45f;

    [SerializeField]
    private float minimumSpacing = 8f;

    [SerializeField]
    [Range(0f, 0.5f)]
    private float emptySpace = 0.3f;

    [SerializeField]
    [Range(0f, 0.4f)]
    private float positionVariation = 0.25f;

    [SerializeField]
    private float outerTopMargin = 30f;

    [SerializeField]
    private float resourceRegenerationTime = 10f;

    [SerializeField]
    private float emptyAreaRadius = 26f;

    [SerializeField]
    private Vector2[] emptyAreaCenters = new Vector2[]
    {
        new Vector2(-210f, 90f),
        new Vector2(-175f, -80f),
        new Vector2(185f, 70f),
        new Vector2(225f, -95f)
    };

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
            float slotCenter = -edgeX + horizontalSpacing * (i + 0.5f);
            float variation = horizontalSpacing * 0.3f;

            float northX = slotCenter + GetRandomNumber(-variation, variation);
            float southX = slotCenter + GetRandomNumber(-variation, variation);

            float northZ = edgeZ - GetRandomNumber(0f, mountainBandDepth);
            float southZ = -edgeZ + GetRandomNumber(0f, mountainBandDepth);

            CreateMountain(northX, northZ, true);
            CreateMountain(southX, southZ, true);
        }

        for (int i = 0; i < mountainsPerShortSide; i++)
        {
            float slotCenter = -edgeZ + verticalSpacing * (i + 0.5f);
            float variation = verticalSpacing * 0.3f;

            float eastZ = slotCenter + GetRandomNumber(-variation, variation);
            float westZ = slotCenter + GetRandomNumber(-variation, variation);

            float eastX = edgeX - GetRandomNumber(0f, mountainBandDepth);
            float westX = -edgeX + GetRandomNumber(0f, mountainBandDepth);

            CreateMountain(eastX, eastZ, false);
            CreateMountain(westX, westZ, false);
        }
    }

    private void CreateMountain(float x, float z, bool nearRiver)
    {
        x += GetRandomNumber(-6f, 6f);
        z += GetRandomNumber(-6f, 6f);

        if (nearRiver && Mathf.Abs(x) < riverMountainClearance)
        {
            if (x < 0f)
            {
                x = -riverMountainClearance;
            }
            else
            {
                x = riverMountainClearance;
            }
        }

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

        float width = GetRandomNumber(0.7f, 1.25f);
        float heightScale = GetRandomNumber(0.8f, 1.3f);
        float depth = GetRandomNumber(0.7f, 1.25f);
        Vector3 templateScale = mountainTemplate.transform.localScale;

        mountainNumber++;
        mountain.transform.localScale = new Vector3(
            templateScale.x * width,
            templateScale.y * heightScale,
            templateScale.z * depth
        );
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
        Vector2 sideSize = new Vector2(sideWidth, mapSize.y - outerTopMargin);
        float horizontalOffset = existingMapSize.x / 2f + sideWidth / 2f;
        float verticalOffset = -outerTopMargin / 2f;

        GenerateOuterArea(new Vector2(horizontalOffset, verticalOffset), sideSize);
        GenerateOuterArea(new Vector2(-horizontalOffset, verticalOffset), sideSize);
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
        int totalAmount = treeAmount + bushAmount + rockAmount;
        List<Vector2> positions = CreateNaturalPositions(center, size, totalAmount);

        MixPositions(positions);

        CreateResources(treeTemplate, ResourceType.Wood, treeAmount, positions, 0);
        CreateResources(bushTemplate, ResourceType.Wood, bushAmount, positions, treeAmount);
        CreateResources(
            rockTemplate,
            ResourceType.Stone,
            rockAmount,
            positions,
            treeAmount + bushAmount
        );
    }

    private void CreateResources(
        GameObject resourceTemplate,
        ResourceType resourceType,
        int amount,
        List<Vector2> positions,
        int firstPosition
    )
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 mapPosition = positions[firstPosition + i];
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

            float width = GetRandomNumber(0.75f, 1.2f);
            float heightScale = GetRandomNumber(0.85f, 1.3f);
            Vector3 templateScale = resourceTemplate.transform.localScale;

            resourceNumber++;
            resource.transform.localScale = new Vector3(
                templateScale.x * width,
                templateScale.y * heightScale,
                templateScale.z * width
            );
            resource.name = resourceTemplate.name + "_Generated_" + resourceNumber;

            if (Application.isPlaying)
            {
                MakeCollectible(resource, resourceType);
            }
            else
            {
                HidePreviewObject(resource);
            }

        }
    }

    private List<Vector2> CreateNaturalPositions(Vector2 center, Vector2 size, int amount)
    {
        List<Vector2> positions = new List<Vector2>();
        List<Vector2> availablePositions = new List<Vector2>();

        float areaShape = size.x / size.y;
        int cellAmount = Mathf.CeilToInt(amount * (1f + emptySpace));
        int columns = Mathf.CeilToInt(Mathf.Sqrt(cellAmount * areaShape));
        int rows = Mathf.CeilToInt((float)cellAmount / columns);

        float cellWidth = size.x / columns;
        float cellHeight = size.y / rows;
        float left = center.x - size.x / 2f;
        float bottom = center.y - size.y / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float x = left + cellWidth * (column + 0.5f);
                float z = bottom + cellHeight * (row + 0.5f);

                x += GetRandomNumber(-cellWidth * positionVariation, cellWidth * positionVariation);
                z += GetRandomNumber(-cellHeight * positionVariation, cellHeight * positionVariation);

                availablePositions.Add(new Vector2(x, z));
            }
        }

        MixPositions(availablePositions);

        foreach (Vector2 position in availablePositions)
        {
            if (positions.Count == amount)
            {
                break;
            }

            if (IsPositionFree(position))
            {
                positions.Add(position);
                usedPositions.Add(position);
            }
        }

        while (positions.Count < amount)
        {
            Vector2 position = FindFreePosition(center, size);
            positions.Add(position);
            usedPositions.Add(position);
        }

        return positions;
    }

    private void MixPositions(List<Vector2> positions)
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int otherPosition = random.Next(i + 1);
            Vector2 savedPosition = positions[i];

            positions[i] = positions[otherPosition];
            positions[otherPosition] = savedPosition;
        }
    }

    private Vector2 FindFreePosition(Vector2 center, Vector2 size)
    {
        float left = center.x - size.x / 2f;
        float right = center.x + size.x / 2f;
        float bottom = center.y - size.y / 2f;
        float top = center.y + size.y / 2f;

        Vector2 position = center;

        for (int attempt = 0; attempt < 100; attempt++)
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
        foreach (Vector2 emptyAreaCenter in emptyAreaCenters)
        {
            if (Vector2.Distance(position, emptyAreaCenter) < emptyAreaRadius)
            {
                return false;
            }
        }

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
        staticResource.Configure(
            resourceType,
            false,
            resourceRegenerationTime
        );
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
