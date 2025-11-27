using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using F1RaceAnalytics.Models;
using F1RaceAnalytics.Services;
using System.Text.Json;

namespace F1RaceAnalytics.Pages;

public partial class RaceResults : ComponentBase
{
    [Parameter]
    public int SessionKey { get; set; }

    [Inject]
    private OpenF1Service OpenF1Service { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private List<DriverStanding> driverStandings = new();
    private string raceTitle = "";
    private string raceDate = "";
    private bool isLoading = true;
    private string? errorMessage;
    private string? chartDataJson;
    private string? lapTimesJson;
    private string trackImageUrl = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadRaceData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(chartDataJson))
        {
            await JSRuntime.InvokeVoidAsync("initializeCharts", chartDataJson, lapTimesJson);
        }
    }

    private async Task LoadRaceData()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var sessions = await OpenF1Service.GetSessionsAsync(2022, 2025);
            var currentSession = sessions.FirstOrDefault(s => s.SessionKey == SessionKey);

            if (currentSession == null)
            {
                errorMessage = "Session not found.";
                return;
            }

            raceTitle = $"{currentSession.CountryName} Grand Prix";
            raceDate = currentSession.DateStart.ToString("MMMM dd, yyyy");
            trackImageUrl = GetTrackImageUrl(currentSession.CircuitShortName);

            var drivers = await OpenF1Service.GetDriversAsync(SessionKey);
            var positions = await OpenF1Service.GetPositionsAsync(SessionKey);
            var pitStops = await OpenF1Service.GetPitStopsAsync(SessionKey);
            var laps = await OpenF1Service.GetLapsAsync(SessionKey);
            var stints = await OpenF1Service.GetStintsAsync(SessionKey);

            var driverGroups = positions
                .GroupBy(p => p.DriverNumber)
                .Select(g => new
                {
                    DriverNumber = g.Key,
                    FinalPosition = g.OrderBy(p => p.Date).Last().Position,
                    StartPosition = g.OrderBy(p => p.Date).First().Position,
                    Positions = g.OrderBy(p => p.Date).ToList()
                })
                .OrderBy(d => d.FinalPosition)
                .ToList();

            driverStandings = driverGroups.Select(dg =>
            {
                var driver = drivers.FirstOrDefault(d => d.DriverNumber == dg.DriverNumber);
                var driverLaps = laps.Where(l => l.DriverNumber == dg.DriverNumber).ToList();
                var driverPitStops = pitStops.Count(ps => ps.DriverNumber == dg.DriverNumber);
                var driverStints = stints.Where(s => s.DriverNumber == dg.DriverNumber)
                    .OrderBy(s => s.LapStart)
                    .Select(s => new TireStint
                    {
                        Compound = s.Compound,
                        Laps = (s.LapEnd ?? 0) - s.LapStart + 1
                    })
                    .ToList();

                var bestLap = driverLaps.Where(l => l.LapDuration.HasValue && l.LapDuration.Value > 0)
                    .OrderBy(l => l.LapDuration)
                    .FirstOrDefault();

                return new DriverStanding
                {
                    Position = dg.FinalPosition,
                    DriverNumber = dg.DriverNumber,
                    DriverName = driver?.FullName ?? $"Driver {dg.DriverNumber}",
                    TeamName = driver?.TeamName ?? "Unknown",
                    TeamColour = driver?.TeamColour ?? "FFFFFF",
                    HeadshotUrl = driver?.HeadshotUrl,
                    PitStops = driverPitStops,
                    BestLapTime = bestLap?.LapDuration ?? 0,
                    PositionDelta = dg.StartPosition - dg.FinalPosition,
                    TireStints = driverStints
                };
            }).ToList();

            if (driverStandings.Any(d => d.BestLapTime > 0))
            {
                var fastestLap = driverStandings.Where(d => d.BestLapTime > 0).Min(d => d.BestLapTime);
                var fastestDriver = driverStandings.FirstOrDefault(d => d.BestLapTime == fastestLap);
                if (fastestDriver != null)
                {
                    fastestDriver.HasFastestLap = true;
                }
            }

            var positionChartData = new
            {
                drivers = driverGroups.Select(dg =>
                {
                    var driver = drivers.FirstOrDefault(d => d.DriverNumber == dg.DriverNumber);
                    return new
                    {
                        driverNumber = dg.DriverNumber,
                        driverName = driver?.NameAcronym ?? $"DR{dg.DriverNumber}",
                        teamColor = driver?.TeamColour ?? "FFFFFF",
                        positions = dg.Positions.Select(p => new
                        {
                            lap = 0,
                            position = p.Position
                        }).ToList()
                    };
                }).ToList()
            };

            chartDataJson = JsonSerializer.Serialize(positionChartData);

            var lapTimesData = new
            {
                drivers = driverGroups.Select(dg =>
                {
                    var driver = drivers.FirstOrDefault(d => d.DriverNumber == dg.DriverNumber);
                    var driverLaps = laps.Where(l => l.DriverNumber == dg.DriverNumber && l.LapDuration.HasValue && l.LapDuration.Value > 0)
                        .OrderBy(l => l.LapNumber)
                        .ToList();

                    return new
                    {
                        driverNumber = dg.DriverNumber,
                        driverName = driver?.NameAcronym ?? $"DR{dg.DriverNumber}",
                        teamColor = driver?.TeamColour ?? "FFFFFF",
                        lapTimes = driverLaps.Select(l => new
                        {
                            lap = l.LapNumber,
                            time = l.LapDuration!.Value
                        }).ToList()
                    };
                }).ToList()
            };

            lapTimesJson = JsonSerializer.Serialize(lapTimesData);
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"Error loading race data: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            errorMessage = "Request timed out. Please try again.";
        }
        catch (JsonException ex)
        {
            errorMessage = $"Error processing race data: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = $"Error loading race data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string FormatLapTime(double seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
    }

    private static string GetTrackImageUrl(string circuitName)
    {
        var trackMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Monaco"] = "Monte_Carlo_Circuit",
            ["Monza"] = "Monza_Circuit",
            ["Silverstone"] = "Great_Britain_Circuit",
            ["Spa-Francorchamps"] = "Spa_Circuit",
            ["Suzuka"] = "Suzuka_Circuit",
            ["Circuit of the Americas"] = "Miami_Circuit",
            ["Austin"] = "Miami_Circuit",
            ["Interlagos"] = "Brazil_Circuit",
            ["Catalunya"] = "Spain_Circuit",
            ["Barcelona"] = "Spain_Circuit",
            ["Red Bull Ring"] = "Austria_Circuit",
            ["Spielberg"] = "Austria_Circuit",
            ["Zandvoort"] = "Zandvoort_Circuit",
            ["Hungaroring"] = "Hungary_Circuit",
            ["Imola"] = "Emilia_Romagna_Circuit",
            ["Miami"] = "Miami_Circuit",
            ["Jeddah"] = "Saudi_Arabia_Circuit",
            ["Melbourne"] = "Australia_Circuit",
            ["Albert Park"] = "Australia_Circuit",
            ["Sakhir"] = "Bahrain_Circuit",
            ["Bahrain"] = "Bahrain_Circuit",
            ["Shanghai"] = "China_Circuit",
            ["Baku"] = "Azerbaijan_Circuit",
            ["Azerbaijan"] = "Azerbaijan_Circuit",
            ["Montreal"] = "Canada_Circuit",
            ["Marina Bay"] = "Singapore_Circuit",
            ["Singapore"] = "Singapore_Circuit",
            ["Lusail"] = "Qatar_Circuit",
            ["Yas Marina"] = "Abu_Dhabi_Circuit",
            ["Las Vegas"] = "Las_Vegas_Circuit",
            ["Mexico City"] = "Mexico_Circuit"
        };

        if (trackMap.TryGetValue(circuitName, out var fileName))
        {
            return $"https://media.formula1.com/image/upload/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/{fileName}.png";
        }

        return "";
    }

    private static int GetRacePoints(int position)
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
}