// Keep the existing smoke-check entry points while testing the current city-wide
// event model instead of the removed prototype-neighbourhood assumptions.
public static class CitySceneSmokeCheck
{
    public static void Run() => CityWideEventsCheck.RunSource();
    public static void RunIntegrated() => CityWideEventsCheck.Run();
}
