namespace WindApi.Models;

public class WeatherRequest
{
    public string LocationNw { get; set; } = null!;
    public string LocationNe { get; set; } = null!;
    public string LocationSw { get; set; } = null!;
    public string LocationSe { get; set; } = null!;
    public int ResolutionX { get; set; }
    public int ResolutionY { get; set; }
    public TimeSpan TimeAhead { get; set; }
    
    public string[]? Poi { get; set; }
    public int? SamplePoints { get; set; }
}
