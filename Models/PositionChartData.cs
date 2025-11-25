namespace F1RaceAnalytics.Models;

public class PositionChartData
{
    public int DriverNumber { get; set; }
    public string DriverAcronym { get; set; } = string.Empty;
    public string TeamColour { get; set; } = string.Empty;
    public int Lap { get; set; }
    public int Position { get; set; }
}