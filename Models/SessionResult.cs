namespace F1RaceAnalytics.Models;

public class SessionResult
{
    public int DriverNumber { get; set; }
    public int Position { get; set; }
    public bool Dnf { get; set; }
    public bool Dns { get; set; }
    public bool Dsq { get; set; }
}