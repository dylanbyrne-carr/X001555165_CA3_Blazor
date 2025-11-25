namespace F1RaceAnalytics.Models;

public class DriverStanding
{
    public int Position { get; set; }
    public int StartPosition { get; set; }
    public Driver Driver { get; set; } = new();
    public int Points { get; set; }
    public int PositionDelta { get; set; }
    public bool Dnf { get; set; }
    public bool Dns { get; set; }
    public bool Dsq { get; set; }
}