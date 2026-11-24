using System.Collections.Generic;
using UnityEngine;

public class Cities
{

    public static Dictionary<string, CityData> cities = new Dictionary<string, CityData>()
    {
        {"PLAYER", new PlayerInfo()},
        {"AI", new CityData()},
    };
      
}
