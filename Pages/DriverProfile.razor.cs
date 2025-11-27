using Microsoft.AspNetCore.Components;
using F1RaceAnalytics.Models;
using F1RaceAnalytics.Services;

namespace F1RaceAnalytics.Pages;

public partial class DriverProfile
{
    [Parameter]
    public int DriverNumber { get; set; }

    [Inject]
    private OpenF1Service OpenF1Service { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private Driver? driver;
    private DriverStats allStats = new();
    private Dictionary<int, DriverStats> seasonStats = new();
    private bool isLoading = true;
    private bool isLoadingStats;
    private string? errorMessage;
    
    private int currentRaceIndex;
    private int totalRaces;
    private string currentRaceName = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadDriverData();
    }

    private async Task LoadDriverData()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var allSessions = new List<Session>();
            
            for (int year = 2022; year <= 2025; year++)
            {
                var sessions = await OpenF1Service.GetSessionsAsync(year);
                
                // Filter out Sprint sessions
                var regularRaces = sessions.Where(s => 
                    s.SessionType == "Race" && 
                    (s.SessionName == null || !s.SessionName.Contains("Sprint", StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                allSessions.AddRange(regularRaces);
            }

            var latestSession = allSessions
                .Where(s => s.SessionType == "Race")
                .OrderByDescending(s => s.DateStart)
                .FirstOrDefault();

            if (latestSession != null)
            {
                var drivers = await OpenF1Service.GetDriversAsync(latestSession.SessionKey);
                driver = drivers.FirstOrDefault(d => d.DriverNumber == DriverNumber);
            }

            if (driver == null)
            {
                errorMessage = "Driver not found.";
                return;
            }

            isLoading = false;
            isLoadingStats = true;
            StateHasChanged();

            var raceSessions = allSessions
                .Where(s => s.SessionType == "Race")
                .OrderBy(s => s.DateStart)
                .ToList();

            totalRaces = raceSessions.Count;
            var allPositions = new List<int>();

            foreach (var year in new[] { 2025, 2024, 2023, 2022 })
            {
                var yearSessions = raceSessions.Where(s => s.Year == year).ToList();
                var yearStats = new DriverStats();
                var yearPositions = new List<int>();

                foreach (var session in yearSessions)
                {
                    try
                    {
                        currentRaceIndex++;
                        currentRaceName = $"{session.CountryName} - {session.CircuitShortName}";
                        StateHasChanged();

                        await Task.Delay(100);
                        
                        var positionsData = await OpenF1Service.GetPositionsAsync(session.SessionKey);
                        var driverPositions = positionsData.Where(p => p.DriverNumber == DriverNumber).ToList();
                        
                        if (driverPositions.Count > 0)
                        {
                            var finalPosition = driverPositions.OrderBy(p => p.Date).Last().Position;
                            
                            if (finalPosition > 0)
                            {
                                yearStats.TotalRaces++;
                                allStats.TotalRaces++;
                                
                                yearPositions.Add(finalPosition);
                                allPositions.Add(finalPosition);
                                
                                var points = GetPointsForPosition(finalPosition);
                                yearStats.Points += points;
                                allStats.Points += points;
                                
                                if (finalPosition <= 3)
                                {
                                    yearStats.Podiums++;
                                    allStats.Podiums++;
                                }
                            }
                        }

                        if (allPositions.Count > 0)
                        {
                            allStats.BestPosition = allPositions.Min();
                            allStats.WorstPosition = allPositions.Max();
                            allStats.AveragePosition = allPositions.Average();
                        }

                        StateHasChanged();
                    }
                    catch (HttpRequestException)
                    {
                        continue;
                    }
                    catch (TaskCanceledException)
                    {
                        continue;
                    }
                }

                if (yearPositions.Count > 0)
                {
                    yearStats.BestPosition = yearPositions.Min();
                    yearStats.WorstPosition = yearPositions.Max();
                    yearStats.AveragePosition = yearPositions.Average();
                }

                if (yearStats.TotalRaces > 0)
                {
                    seasonStats[year] = yearStats;
                }

                StateHasChanged();
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"Error loading driver data: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            errorMessage = "Request timed out. Please try again.";
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = $"Error loading driver data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            isLoadingStats = false;
        }
    }

    private static int GetPointsForPosition(int position)
    {
        return position switch
        {
            1 => 25,
            2 => 18,
            3 => 15,
            4 => 12,
            5 => 10,
            6 => 8,
            7 => 6,
            8 => 4,
            9 => 2,
            10 => 1,
            _ => 0
        };
    }

    private static string GetCountryFlag(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode)) return "";
        return $"https://flagcdn.com/w320/{countryCode.ToLowerInvariant()}.png";
    }
}