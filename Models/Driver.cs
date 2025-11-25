namespace F1RaceAnalytics.Models;

public class Driver
{
    public int DriverNumber { get; set; }
    public string BroadcastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NameAcronym { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string TeamColour { get; set; } = string.Empty;
    public string? HeadshotUrl { get; set; }
}