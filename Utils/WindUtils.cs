using CoordinateSharp;

namespace WindApi.Utils;

public static class WindUtils
{
    public static Coordinate Clone(this Coordinate coord)
    {
        return new Coordinate(coord.Latitude.ToDouble(), coord.Longitude.ToDouble());
    }
}