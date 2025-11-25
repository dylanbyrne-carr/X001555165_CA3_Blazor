using Microsoft.AspNetCore.Components;
using F1RaceAnalytics.Models;
using F1RaceAnalytics.Services;

namespace F1RaceAnalytics.Pages;

public partial class Home
{
    [Inject]
    private OpenF1Service OpenF1Service { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private Dictionary<int, List<TrackInfo>> tracksByYear = new();
    private HashSet<int> expandedYears = new() { 2025 };
    private bool isLoading = true;
    private string? errorMessage = null;
    private List<Session> allSessions = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadAllData();
    }

    private async Task LoadAllData()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var allMeetings = new List<Meeting>();
            allSessions = new List<Session>();
            
            for (int year = 2023; year <= 2025; year++)
            {
                var meetings = await OpenF1Service.GetMeetingsAsync(year);
                var sessions = await OpenF1Service.GetSessionsAsync(year);
                
                allMeetings.AddRange(meetings);
                allSessions.AddRange(sessions);
            }

            tracksByYear = allMeetings
                .GroupBy(m => m.Year)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new TrackInfo
                    {
                        MeetingKey = m.MeetingKey,
                        MeetingOfficialName = m.MeetingOfficialName,
                        CircuitShortName = m.CircuitShortName,
                        CountryName = m.CountryName,
                        Year = m.Year,
                        ImageUrl = GetTrackImageUrl(m.CircuitShortName)
                    }).ToList()
                );
        }
        catch (Exception ex)
        {
            errorMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ToggleYear(int year)
    {
        if (expandedYears.Contains(year))
        {
            expandedYears.Remove(year);
        }
        else
        {
            expandedYears.Add(year);
        }
    }

    private void ViewRaceResults(TrackInfo track)
    {
        var raceSession = allSessions.FirstOrDefault(s => 
            s.MeetingKey == track.MeetingKey && 
            s.SessionType == "Race");

        if (raceSession != null)
        {
            Navigation.NavigateTo($"/race/{raceSession.SessionKey}");
        }
        else
        {
            errorMessage = $"No race session found for {track.CircuitShortName}";
        }
    }

    private string GetTrackImageUrl(string circuitShortName)
    {
        var trackMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Monaco"] = "Monaco_Circuit",
            ["Monte Carlo"] = "Monaco_Circuit",
            ["Monza"] = "Italy_Circuit",
            ["Silverstone"] = "Great_Britain_Circuit",
            ["Spa-Francorchamps"] = "Belgium_Circuit",
            ["Suzuka"] = "Japan_Circuit",
            ["Austin"] = "USA_Circuit",
            ["Interlagos"] = "Brazil_Circuit",
            ["Catalunya"] = "Spain_Circuit",
            ["Barcelona"] = "Spain_Circuit",
            ["Red Bull Ring"] = "Austria_Circuit",
            ["Spielberg"] = "Austria_Circuit",
            ["Zandvoort"] = "Netherlands_Circuit",
            ["Hungaroring"] = "Hungary_Circuit",
            ["Imola"] = "Emilia_Romagna_Circuit",
            ["Miami"] = "Miami_Circuit",
            ["Jeddah"] = "Saudi_Arabia_Circuit",
            ["Melbourne"] = "Australia_Circuit",
            ["Albert Park"] = "Australia_Circuit",
            ["Sakhir"] = "Bahrain_Circuit",
            ["Bahrain"] = "Bahrain_Circuit",
            ["Shanghai"] = "China_Circuit",
            ["Baku"] = "Baku_Circuit", // godhelpme whichone is the right baku
            ["Azerbaijan"] = "Azerbaijan_Circuit",
            ["Montreal"] = "Canada_Circuit",
            ["Marina Bay"] = "Singapore_Circuit",
            ["Singapore"] = "Singapore_Circuit",
            ["Lusail"] = "Qatar_Circuit",
            ["Yas Marina"] = "Abu_Dhabi_Circuit",
            ["Las Vegas"] = "Las_Vegas_Circuit",
            ["Mexico City"] = "Mexico_Circuit"
        };

        var fileName = trackMap.FirstOrDefault(kvp => 
            circuitShortName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
            circuitShortName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)).Value;
        
        if (fileName != null)
        {
            return $"https://media.formula1.com/image/upload/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/{fileName}.png";
        }

        return "";
    }
}