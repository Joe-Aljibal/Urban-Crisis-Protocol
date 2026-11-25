public enum TownType { Human, AI }

// Compatibility view over each city's lifetime delivered resources.
public static class TownResourceStatistics
{
    private static CityData City(TownType town) => Cities.cities[town == TownType.Human ? "PLAYER" : "AI"];
    public static void AddResource(TownType town, ResourceType type, int amount) => City(town).addResource(type, amount);
    public static int GetWood(TownType town) => City(town).getExploitedWood;
    public static int GetStone(TownType town) => City(town).getExploitedStone;
    public static int GetTotal(TownType town) => GetWood(town) + GetStone(town);
}
