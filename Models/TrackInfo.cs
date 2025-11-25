namespace F1RaceAnalytics.Models;

public class TrackInfo
{
    public int MeetingKey { get; set; }
    public string MeetingOfficialName { get; set; } = string.Empty;
    public string CircuitShortName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public int Year { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}