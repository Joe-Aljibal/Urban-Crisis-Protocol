public enum TownType
{
    Human,
    AI
}

public static class TownResourceStatistics
{
    private static int humanWood;
    private static int humanStone;
    private static int aiWood;
    private static int aiStone;

    public static void AddResource(TownType town, ResourceType resourceType, int amount)
    {
        if (town == TownType.Human)
        {
            if (resourceType == ResourceType.Wood)
                humanWood += amount;
            else if (resourceType == ResourceType.Stone)
                humanStone += amount;

            return;
        }

        if (resourceType == ResourceType.Wood)
            aiWood += amount;
        else if (resourceType == ResourceType.Stone)
            aiStone += amount;
    }

    public static int GetWood(TownType town)
    {
        return town == TownType.Human ? humanWood : aiWood;
    }

    public static int GetStone(TownType town)
    {
        return town == TownType.Human ? humanStone : aiStone;
    }

    public static int GetTotal(TownType town)
    {
        return GetWood(town) + GetStone(town);
    }
}
