using System.Text.Json;
using F1RaceAnalytics.Models;

namespace F1RaceAnalytics.Services;

public class OpenF1Service
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenF1Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<List<T>> FetchAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
            return data ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching {typeof(T).Name}: {ex.Message}");
            return [];
        }
    }

    public Task<List<Session>> GetSessionsAsync(int year, string? countryName = null)
    {
        var url = $"sessions?year={year}";
        if (!string.IsNullOrEmpty(countryName))
        {
            url += $"&country_name={Uri.EscapeDataString(countryName)}";
        }
        return FetchAsync<Session>(url);
    }

    public Task<List<Meeting>> GetMeetingsAsync(int year)
    {
        var url = $"meetings?year={year}";
        return FetchAsync<Meeting>(url);
    }

    public Task<List<Driver>> GetDriversAsync(int sessionKey)
    {
        var url = $"drivers?session_key={sessionKey}";
        return FetchAsync<Driver>(url);
    }

    public Task<List<PositionData>> GetPositionsAsync(int sessionKey)
    {
        var url = $"position?session_key={sessionKey}";
        return FetchAsync<PositionData>(url);
    }

    public Task<List<Lap>> GetLapsAsync(int sessionKey)
    {
        var url = $"laps?session_key={sessionKey}";
        return FetchAsync<Lap>(url);
    }

    public Task<List<SessionResult>> GetSessionResultsAsync(int sessionKey)
    {
        var url = $"session_results?session_key={sessionKey}";
        return FetchAsync<SessionResult>(url);
    }

    public Task<List<Stint>> GetStintsAsync(int sessionKey)
    {
        var url = $"stints?session_key={sessionKey}";
        return FetchAsync<Stint>(url);
    }

    public Task<List<PitStop>> GetPitStopsAsync(int sessionKey)
    {
        var url = $"pit?session_key={sessionKey}";
        return FetchAsync<PitStop>(url);
    }

    public async Task<Session?> GetSessionAsync(int sessionKey)
    {
        var url = $"sessions?session_key={sessionKey}";
        var sessions = await FetchAsync<Session>(url);
        return sessions.FirstOrDefault();
    }
}