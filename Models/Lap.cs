namespace F1RaceAnalytics.Models;

public class Lap
{
    public int DriverNumber { get; set; }
    public int LapNumber { get; set; }
    public double? LapDuration { get; set; }
    public bool IsPitOutLap { get; set; }
    public double? Sector1Duration { get; set; }
    public double? Sector2Duration { get; set; }
    public double? Sector3Duration { get; set; }
}