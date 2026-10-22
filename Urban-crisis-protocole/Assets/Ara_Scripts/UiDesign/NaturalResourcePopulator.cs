using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NaturalResourcePopulator : MonoBehaviour
{
    private const string GeneratedName = "_Generated_";
    private const float Half = 0.5f;
    private const float FullRotation = 360f;
    private const float MountainSlotVariation = 0.3f;
    private const float MountainPositionVariation = 6f;
    private const float MountainMinimumWidth = 0.7f;
    private const float MountainMaximumWidth = 1.25f;
    private const float MountainMinimumHeight = 0.8f;
    private const float MountainMaximumHeight = 1.3f;
    private const float ResourceMinimumWidth = 0.75f;
    private const float ResourceMaximumWidth = 1.2f;
    private const float ResourceMinimumHeight = 0.85f;
    private const float ResourceMaximumHeight = 1.3f;
    private const int MaximumPositionAttempts = 100;

    [SerializeField] private GameObject treeTemplate;
    [SerializeField] private GameObject bushTemplate;
    [SerializeField] private GameObject rockTemplate;
    [SerializeField] private GameObject mountainTemplate;
    [SerializeField] private Transform humanTown;
    [SerializeField] private Transform aiTown;
    [SerializeField] private Transform mountainParent;
    [SerializeField] private Vector2 territorySize = new Vector2(60f, 75f);
    [SerializeField] private float resourceBandWidth = 40f;
    [SerializeField] private int treesPerSide = 5;
    [SerializeField] private int bushesPerSide = 2;
    [SerializeField] private int rocksPerSide = 1;
    [SerializeField] private Vector2 mapSize = new Vector2(540f, 350f);
    [SerializeField] private Vector2 existingMapSize = new Vector2(230f, 155f);
    [SerializeField] private int outerTreesPerArea = 72;
    [SerializeField] private int outerBushesPerArea = 36;
    [SerializeField] private int outerRocksPerArea = 14;
    [SerializeField] private int mountainsPerLongSide = 8;
    [SerializeField] private int mountainsPerShortSide = 5;
    [SerializeField] private float mountainEdgeInset = 20f;
    [SerializeField] private float mountainBandDepth = 35f;
    [SerializeField] private float riverMountainClearance = 45f;
    [SerializeField] private float minimumSpacing = 8f;
    [SerializeField] private float emptySpace = 0.3f;
    [SerializeField] private float positionVariation = 0.25f;
    [SerializeField] private float outerTopMargin = 30f;
    [SerializeField] private float emptyAreaRadius = 26f;
    [SerializeField] private Vector2[] emptyAreaCenters =
    {
        new Vector2(-210f, 90f),
        new Vector2(-175f, -80f),
        new Vector2(185f, 70f),
        new Vector2(225f, -95f)
    };
    [SerializeField] private int seed = 2026;

    private System.Random random;
    private readonly List<Vector2> usedPositions = new List<Vector2>();
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
        float edgeX = mapSize.x * Half - mountainEdgeInset;
        float edgeZ = mapSize.y * Half - mountainEdgeInset;

        GenerateMountainEdges(mountainsPerLongSide, edgeX, edgeZ, true);
        GenerateMountainEdges(mountainsPerShortSide, edgeZ, edgeX, false);
    }

    private void GenerateMountainEdges(
        int amount,
        float movingEdge,
        float fixedEdge,
        bool horizontal
    )
    {
        float spacing = movingEdge * 2f / amount;

        for (int index = 0; index < amount; index++)
        {
            float center = -movingEdge + spacing * (index + Half);
            float variation = spacing * MountainSlotVariation;
            float firstPosition = center + RandomBetween(-variation, variation);
            float secondPosition = center + RandomBetween(-variation, variation);
            float positiveEdge = fixedEdge - RandomBetween(0f, mountainBandDepth);
            float negativeEdge = -fixedEdge + RandomBetween(0f, mountainBandDepth);

            if (horizontal)
            {
                CreateMountain(firstPosition, positiveEdge, true);
                CreateMountain(secondPosition, negativeEdge, true);
            }
            else
            {
                CreateMountain(positiveEdge, firstPosition, false);
                CreateMountain(negativeEdge, secondPosition, false);
            }
        }
    }

    private void CreateMountain(float x, float z, bool nearRiver)
    {
        x += RandomBetween(-MountainPositionVariation, MountainPositionVariation);
        z += RandomBetween(-MountainPositionVariation, MountainPositionVariation);

        if (nearRiver && Mathf.Abs(x) < riverMountainClearance)
        {
            if (x < 0f)
                x = -riverMountainClearance;
            else
                x = riverMountainClearance;
        }

        Vector3 position = new Vector3(x, mountainTemplate.transform.position.y, z);
        Quaternion rotation = Quaternion.Euler(0f, RandomRotation(), 0f);
        GameObject mountain = Instantiate(
            mountainTemplate,
            position,
            rotation,
            mountainParent
        );

        float width = RandomBetween(MountainMinimumWidth, MountainMaximumWidth);
        float height = RandomBetween(MountainMinimumHeight, MountainMaximumHeight);
        float depth = RandomBetween(MountainMinimumWidth, MountainMaximumWidth);
        Vector3 templateScale = mountainTemplate.transform.localScale;

        mountain.transform.localScale = new Vector3(
            templateScale.x * width,
            templateScale.y * height,
            templateScale.z * depth
        );

        mountainNumber++;
        mountain.name = mountainTemplate.name + GeneratedName + mountainNumber;

        if (!Application.isPlaying)
            HidePreviewObject(mountain);
    }

    private void GenerateResourcesAroundTown(Transform town)
    {
        Vector2 center = new Vector2(town.position.x, town.position.z);
        float horizontalOffset = (territorySize.x + resourceBandWidth) * Half;
        float verticalOffset = (territorySize.y + resourceBandWidth) * Half;
        Vector2 sideSize = new Vector2(resourceBandWidth, territorySize.y);
        Vector2 topSize = new Vector2(
            territorySize.x + resourceBandWidth * 2f,
            resourceBandWidth
        );

        GenerateTownArea(center + new Vector2(horizontalOffset, 0f), sideSize);
        GenerateTownArea(center - new Vector2(horizontalOffset, 0f), sideSize);
        GenerateTownArea(center + new Vector2(0f, verticalOffset), topSize);
        GenerateTownArea(center - new Vector2(0f, verticalOffset), topSize);
    }

    private void GenerateTownArea(Vector2 center, Vector2 size)
    {
        GenerateArea(center, size, treesPerSide, bushesPerSide, rocksPerSide);
    }

    private void GenerateResourcesOnOuterGround()
    {
        float sideWidth = (mapSize.x - existingMapSize.x) * Half;
        float horizontalOffset = (existingMapSize.x + sideWidth) * Half;
        float verticalOffset = -outerTopMargin * Half;
        Vector2 size = new Vector2(sideWidth, mapSize.y - outerTopMargin);

        GenerateOuterArea(new Vector2(horizontalOffset, verticalOffset), size);
        GenerateOuterArea(new Vector2(-horizontalOffset, verticalOffset), size);
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
        GameObject template,
        ResourceType type,
        int amount,
        List<Vector2> positions,
        int firstPosition
    )
    {
        for (int index = 0; index < amount; index++)
        {
            Vector2 mapPosition = positions[firstPosition + index];
            Vector3 position = new Vector3(
                mapPosition.x,
                template.transform.position.y,
                mapPosition.y
            );
            Quaternion rotation = Quaternion.Euler(0f, RandomRotation(), 0f);
            GameObject resource = Instantiate(template, position, rotation, transform);

            float width = RandomBetween(ResourceMinimumWidth, ResourceMaximumWidth);
            float height = RandomBetween(ResourceMinimumHeight, ResourceMaximumHeight);
            Vector3 templateScale = template.transform.localScale;

            resource.transform.localScale = new Vector3(
                templateScale.x * width,
                templateScale.y * height,
                templateScale.z * width
            );

            resourceNumber++;
            resource.name = template.name + GeneratedName + resourceNumber;

            if (Application.isPlaying)
                MakeCollectible(resource, type);
            else
                HidePreviewObject(resource);
        }
    }

    private List<Vector2> CreateNaturalPositions(
        Vector2 center,
        Vector2 size,
        int amount
    )
    {
        List<Vector2> positions = new List<Vector2>();
        List<Vector2> availablePositions = CreateGridPositions(center, size, amount);

        MixPositions(availablePositions);

        foreach (Vector2 position in availablePositions)
        {
            if (positions.Count == amount)
                break;

            if (IsPositionFree(position))
                SavePosition(position, positions);
        }

        while (positions.Count < amount)
            SavePosition(FindFreePosition(center, size), positions);

        return positions;
    }

    private List<Vector2> CreateGridPositions(Vector2 center, Vector2 size, int amount)
    {
        List<Vector2> positions = new List<Vector2>();
        float areaShape = size.x / size.y;
        int cellAmount = Mathf.CeilToInt(amount * (1f + emptySpace));
        int columns = Mathf.CeilToInt(Mathf.Sqrt(cellAmount * areaShape));
        int rows = Mathf.CeilToInt((float)cellAmount / columns);
        float cellWidth = size.x / columns;
        float cellHeight = size.y / rows;
        float left = center.x - size.x * Half;
        float bottom = center.y - size.y * Half;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float x = left + cellWidth * (column + Half);
                float z = bottom + cellHeight * (row + Half);
                float xVariation = cellWidth * positionVariation;
                float zVariation = cellHeight * positionVariation;

                x += RandomBetween(-xVariation, xVariation);
                z += RandomBetween(-zVariation, zVariation);
                positions.Add(new Vector2(x, z));
            }
        }

        return positions;
    }

    private void SavePosition(Vector2 position, List<Vector2> positions)
    {
        positions.Add(position);
        usedPositions.Add(position);
    }

    private void MixPositions(List<Vector2> positions)
    {
        for (int index = positions.Count - 1; index > 0; index--)
        {
            int otherIndex = random.Next(index + 1);
            Vector2 savedPosition = positions[index];
            positions[index] = positions[otherIndex];
            positions[otherIndex] = savedPosition;
        }
    }

    private Vector2 FindFreePosition(Vector2 center, Vector2 size)
    {
        float left = center.x - size.x * Half;
        float right = center.x + size.x * Half;
        float bottom = center.y - size.y * Half;
        float top = center.y + size.y * Half;
        Vector2 position = center;

        for (int attempt = 0; attempt < MaximumPositionAttempts; attempt++)
        {
            position = new Vector2(
                RandomBetween(left, right),
                RandomBetween(bottom, top)
            );

            if (IsPositionFree(position))
                return position;
        }

        return position;
    }

    private bool IsPositionFree(Vector2 position)
    {
        foreach (Vector2 emptyCenter in emptyAreaCenters)
        {
            if (Vector2.Distance(position, emptyCenter) < emptyAreaRadius)
                return false;
        }

        foreach (Vector2 usedPosition in usedPositions)
        {
            if (Vector2.Distance(position, usedPosition) < minimumSpacing)
                return false;
        }

        return true;
    }

    private void MakeCollectible(GameObject resource, ResourceType type)
    {
        foreach (MeshFilter mesh in resource.GetComponentsInChildren<MeshFilter>())
            mesh.gameObject.AddComponent<BoxCollider>();

        StaticResource staticResource = resource.AddComponent<StaticResource>();
        staticResource.Configure(type);
    }

    private void ClearGeneratedObjects()
    {
        RemoveGeneratedChildren(transform);
        RemoveGeneratedChildren(mountainParent);
    }

    private void RemoveGeneratedChildren(Transform parent)
    {
        for (int index = parent.childCount - 1; index >= 0; index--)
        {
            GameObject child = parent.GetChild(index).gameObject;

            if (!child.name.Contains(GeneratedName))
                continue;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private void HidePreviewObject(GameObject previewObject)
    {
        foreach (Transform child in previewObject.GetComponentsInChildren<Transform>())
            child.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    private float RandomRotation()
    {
        return RandomBetween(0f, FullRotation);
    }

    private float RandomBetween(float minimum, float maximum)
    {
        float percentage = (float)random.NextDouble();
        return minimum + (maximum - minimum) * percentage;
    }
}
