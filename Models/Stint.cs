namespace F1RaceAnalytics.Models;

public class Stint
{
    public int DriverNumber { get; set; }
    public int StintNumber { get; set; }
    public string Compound { get; set; } = string.Empty;
    public int LapStart { get; set; }
    public int LapEnd { get; set; }
    public int TyreAgeAtStart { get; set; }
}