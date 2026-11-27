// Compatibility facade: legacy UI/events share the player city's data.
public class PlayerInfo : CityData
{
    public static PlayerInfo Instance => (PlayerInfo)Cities.cities["PLAYER"];
    internal PlayerInfo() { }
}
