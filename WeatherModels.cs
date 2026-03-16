using System.Text.Json.Serialization;

namespace NorthShirahebi;

public class WeatherResponse
{
    public Coord coord { get; set; }
    public WeatherData[] weather { get; set; }
    public string @base { get; set; }
    public MainData main { get; set; }
    public int visibility { get; set; }
    public WindData wind { get; set; }
    public CloudsData clouds { get; set; }
    public long dt { get; set; }
    public SysData sys { get; set; }
    public int timezone { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int cod { get; set; }
    public RainSnowData rain { get; set; }
    public RainSnowData snow { get; set; }
}

public class ForecastResponse
{
    public string cod { get; set; }
    public int message { get; set; }
    public int cnt { get; set; }
    public List<ForecastItem> list { get; set; }
    public CityData city { get; set; }
}

public class WeatherErrorResponse { public string message { get; set; } }

public class Coord { public double lon { get; set; } public double lat { get; set; } }
public class WeatherData { public int id { get; set; } public string main { get; set; } public string description { get; set; } public string icon { get; set; } }
public class MainData 
{ 
    public double temp { get; set; } 
    public double feels_like { get; set; } 
    public double temp_min { get; set; } 
    public double temp_max { get; set; } 
    public int pressure { get; set; } 
    public int humidity { get; set; }
    public int sea_level { get; set; }
    public int grnd_level { get; set; }
    public double temp_kf { get; set; }
}
public class WindData { public double speed { get; set; } public int deg { get; set; } public double gust { get; set; } }
public class CloudsData { public int all { get; set; } }
public class SysData { public int type { get; set; } public int id { get; set; } public string country { get; set; } public long sunrise { get; set; } public long sunset { get; set; } public string pod { get; set; } }
public class RainSnowData 
{ 
    [JsonPropertyName("1h")] public double OneHour { get; set; } 
    [JsonPropertyName("3h")] public double ThreeHour { get; set; } 
}
public class ForecastItem
{
    public long dt { get; set; }
    public MainData main { get; set; }
    public WeatherData[] weather { get; set; }
    public CloudsData clouds { get; set; }
    public WindData wind { get; set; }
    public int visibility { get; set; }
    public double pop { get; set; }
    public SysData sys { get; set; }
    public string dt_txt { get; set; }
    public RainSnowData rain { get; set; }
    public RainSnowData snow { get; set; }
}
public class CityData
{
    public int id { get; set; }
    public string name { get; set; }
    public Coord coord { get; set; }
    public string country { get; set; }
    public int population { get; set; }
    public int timezone { get; set; }
    public long sunrise { get; set; }
    public long sunset { get; set; }
}