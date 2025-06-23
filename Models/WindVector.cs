using CoordinateSharp;

namespace WindApi.Models;

public class WindVector()
{
    public double Direction { get; init; }
    public double Magnitude { get; init; }
    public string Coordinate { get; init; } = null!;

    public void Deconstruct(out double Direction, out double Magnitude, out Coordinate Coordinate)
    {
        Direction = this.Direction;
        Magnitude = this.Magnitude;
        Coordinate = Coordinate.Parse(this.Coordinate);
    }
}