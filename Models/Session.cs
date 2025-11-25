namespace F1RaceAnalytics.Models;

public class Session
{
    public int SessionKey { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public int MeetingKey { get; set; }
    public string Location { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string CircuitShortName { get; set; } = string.Empty;
    public int Year { get; set; }
}