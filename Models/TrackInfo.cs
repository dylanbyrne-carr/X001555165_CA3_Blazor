namespace F1RaceAnalytics.Models;

public class TrackInfo
{
    public string CircuitShortName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<int> AvailableYears { get; set; } = [];
    public List<string> RaceNames { get; set; } = [];
    public int SelectedYear { get; set; }
    public Dictionary<int, Meeting> MeetingsByYear { get; set; } = [];
    public string ImageUrl { get; set; } = string.Empty;
}