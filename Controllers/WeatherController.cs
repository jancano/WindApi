using CoordinateSharp;
using Microsoft.AspNetCore.Mvc;
using WindApi.Interfaces;
using WindApi.Models;
using WindApi.Utils;

namespace WindApi.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherController(IWeatherService weatherService) : ControllerBase
{
    private WindVector?[][] _vecs = null!;
    
    [HttpGet]
    public async Task<IActionResult> GetWeather([FromQuery]WeatherRequest request)
    {
        _vecs = new WindVector[request.ResolutionX][];
        for (var i = 0; i < _vecs.GetLength(0); i++)
        {
            _vecs[i] = new WindVector[request.ResolutionY];
        }

        var coordNe = Coordinate.Parse(request.LocationNe);
        var coordNw = Coordinate.Parse(request.LocationNw);
        var coordSe = Coordinate.Parse(request.LocationSe);
        var unused = Coordinate.Parse(request.LocationSw);
        
        var x = new Distance(coordNe, coordNw, Shape.Ellipsoid);
        var quadX = x.Meters / request.ResolutionX;
        var y = new Distance(coordNe, coordSe, Shape.Ellipsoid);
        var quadY = y.Meters / request.ResolutionY;
        
        coordNe.Move(quadX / 2, x.Bearing, Shape.Ellipsoid);
        coordNe.Move(quadY / 2, y.Bearing, Shape.Ellipsoid);

        if (request.Poi is null)
        {
            request.Poi = GetRandomCoordinates(coordNw, coordSe, request.SamplePoints!.Value).Select(c => c.ToString(new CoordinateFormatOptions(){Format = CoordinateFormatType.Decimal})).ToArray();
        }

        var winds = new WindVector[request.Poi.Length];
        for (var i = 0; i < request.Poi.Length; i++)
        {
            winds[i] = (await weatherService.GetWind(Coordinate.Parse(request.Poi[i]), request.TimeAhead))!;
        }
        
        Parallel.For(0, _vecs.Length, i =>
        {
            for (var j = 0; j < _vecs[i].Length; j++)
            {
                var targetCoord = coordNe.Clone();
                targetCoord.Move(quadX * i, x.Bearing, Shape.Ellipsoid);
                targetCoord.Move(quadY * j, y.Bearing, Shape.Ellipsoid);
                
                var (direction, magnitude) = GetWindDirectionAndMagnitude(targetCoord, winds);
                
                _vecs[i][j] = new WindVector {Coordinate = targetCoord.ToString(new CoordinateFormatOptions(){Format = CoordinateFormatType.Decimal}), Direction = direction, Magnitude = magnitude};
            }
        });
        
        return Ok(_vecs);
    }

    private Coordinate[] GetRandomCoordinates(Coordinate a, Coordinate b, int count)
    {
        var result = new Coordinate[count];
        var random = new Random();

        // Get the min and max values for latitude and longitude
        var minLat = Math.Min(a.Latitude.ToDouble(), b.Latitude.ToDouble());
        var maxLat = Math.Max(a.Latitude.ToDouble(), b.Latitude.ToDouble());
        var minLng = Math.Min(a.Longitude.ToDouble(), b.Longitude.ToDouble());
        var maxLng = Math.Max(a.Longitude.ToDouble(), b.Longitude.ToDouble());

        // Generate random coordinates within the bounds
        for (var i = 0; i < count; i++)
        {
            var randomLat = minLat + (maxLat - minLat) * random.NextDouble();
            var randomLng = minLng + (maxLng - minLng) * random.NextDouble();

            result[i] = new Coordinate(randomLat, randomLng);
        }

        return result;
    }

    private (double direction, double magnitude) GetWindDirectionAndMagnitude(Coordinate point, WindVector[]? winds)
    {
        if (winds is null || winds.Length == 0)
        {
            return (0, 0);
        }

        // Convert wind vectors to components and store with distances
        var windComponents = new List<(double x, double y, double distance)>();
        double totalWeight = 0;

        foreach (var wind in winds)
        {
            var (direction, magnitude, coord) = wind;
        
            // Calculate distance between target point and wind measurement point
            var distance = new Distance(point, coord, Shape.Ellipsoid).Meters;
        
            // Avoid division by zero and reduce influence of very close points
            var weight = distance < 0.1 ? 10 : 1 / distance;
            totalWeight += weight;
        
            // Convert polar coordinates (direction, magnitude) to cartesian (x, y)
            var radians = direction * Math.PI / 180;
            var x = magnitude * Math.Sin(radians);
            var y = magnitude * Math.Cos(radians);
        
            windComponents.Add((x, y, weight));
        }

        // Calculate weighted average of components
        double weightedX = 0;
        double weightedY = 0;

        foreach (var (x, y, weight) in windComponents)
        {
            weightedX += x * weight / totalWeight;
            weightedY += y * weight / totalWeight;
        }

        // Convert back to polar coordinates (direction, magnitude)
        var m = Math.Sqrt(weightedX * weightedX + weightedY * weightedY);
        var d = Math.Atan2(weightedX, weightedY) * 180 / Math.PI;
    
        // Ensure direction is in the range [0, 360)
        d = (d + 360) % 360;

        return (d, m);
    }
}