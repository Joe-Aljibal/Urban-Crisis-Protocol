using System.Collections.Generic;
using UnityEngine;

public class Cities
{

    public static Dictionary<string, CityData> cities = new Dictionary<string, CityData>()
    {
        {"PLAYER", new PlayerInfo()},
        {"AI", new CityData()},
    };

    public static void ResetForNewGame()
    {
        foreach (CityData city in cities.Values) city.ResetForNewGame();
    }
}
