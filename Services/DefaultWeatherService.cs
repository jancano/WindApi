using System.Text.Json;
using System.Text.Json.Serialization;
using CoordinateSharp;
using JetBrains.Annotations;
using WindApi.Interfaces;
using WindApi.Models;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace WindApi.Services;

public class DefaultWeatherService(OpenWeatherConfig config) : IWeatherService
{
    public async Task<WindVector?> GetWind(Coordinate coord, TimeSpan? timeAhead)
    {
        if (timeAhead is null)
            timeAhead = TimeSpan.Zero;
            
        HttpClient client = new();
        var response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/forecast?lat={coord.Latitude.ToDouble()}&lon={coord.Longitude.ToDouble()}&appid={config.ApiKey}");
        if (!response.IsSuccessStatusCode)
            return null;
        
        var res = JsonSerializer.Deserialize<WeatherResponse>(await response.Content.ReadAsStringAsync());
        var direction = res?.List[0].Wind.Deg;
        var magnitude = res?.List[0].Wind.Speed * 1.94384449244; //To knots
        return new WindVector() {Coordinate = coord.ToString(new CoordinateFormatOptions(){Format = CoordinateFormatType.Decimal}), Direction = (double)direction!, Magnitude = (double)magnitude!};
    }

    public class WeatherResponse
    {
        [JsonPropertyName("cod")] public string Cod { get; set; }

        [JsonPropertyName("message")] public int Message { get; set; }

        [JsonPropertyName("cnt")] public int Cnt { get; set; }

        [JsonPropertyName("list")] public List<ForecastItem> List { get; set; }

        [JsonPropertyName("city")] public City City { get; set; }
    }

    public class ForecastItem
    {
        [JsonPropertyName("dt")] public long Dt { get; set; }

        [JsonPropertyName("main")] public MainInfo Main { get; set; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        [JsonPropertyName("weather")] public List<Weather> Weather { get; set; }

        [JsonPropertyName("clouds")] public Clouds Clouds { get; set; }

        [JsonPropertyName("wind")] public Wind Wind { get; [UsedImplicitly] set; }

        [JsonPropertyName("visibility")] public int Visibility { get; set; }

        [JsonPropertyName("pop")] public double Pop { get; set; }

        [JsonPropertyName("rain")] public Rain Rain { get; set; }

        [JsonPropertyName("sys")] public Sys Sys { get; set; }

        [JsonPropertyName("dt_txt")] public string DtTxt { get; set; }
    }

    public class MainInfo
    {
        [JsonPropertyName("temp")] public double Temp { get; set; }

        [JsonPropertyName("feels_like")] public double FeelsLike { get; set; }

        [JsonPropertyName("temp_min")] public double TempMin { get; set; }

        [JsonPropertyName("temp_max")] public double TempMax { get; set; }

        [JsonPropertyName("pressure")] public int Pressure { get; set; }

        [JsonPropertyName("sea_level")] public int SeaLevel { get; set; }

        [JsonPropertyName("grnd_level")] public int GrndLevel { get; set; }

        [JsonPropertyName("humidity")] public int Humidity { get; set; }

        [JsonPropertyName("temp_kf")] public double TempKf { get; set; }
    }

    public class Weather
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("main")] public string Main { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("icon")] public string Icon { get; set; }
    }

    public class Clouds
    {
        [JsonPropertyName("all")] public int All { get; set; }
    }

    public class Wind
    {
        [JsonPropertyName("speed")] public double Speed { get; set; }

        [JsonPropertyName("deg")] public int Deg { get; set; }

        [JsonPropertyName("gust")] public double Gust { get; set; }
    }

    public class Rain
    {
        [JsonPropertyName("3h")] public double ThreeHours { get; set; }
    }

    public class Sys
    {
        [JsonPropertyName("pod")] public string Pod { get; set; }
    }

    public class City
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("coord")] public Coord Coord { get; set; }

        [JsonPropertyName("country")] public string Country { get; set; }

        [JsonPropertyName("population")] public int Population { get; set; }

        [JsonPropertyName("timezone")] public int Timezone { get; set; }

        [JsonPropertyName("sunrise")] public long Sunrise { get; set; }

        [JsonPropertyName("sunset")] public long Sunset { get; set; }
    }

    public class Coord
    {
        [JsonPropertyName("lat")] public double Lat { get; set; }

        [JsonPropertyName("lon")] public double Lon { get; set; }
    }
}