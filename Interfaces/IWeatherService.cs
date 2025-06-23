using CoordinateSharp;
using WindApi.Models;

namespace WindApi.Interfaces;

public interface IWeatherService
{
    Task<WindVector?> GetWind(Coordinate coord, TimeSpan? timeAhead);
}