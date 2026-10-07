using System.Net.Http.Json;

namespace WorldCupPredictor.Web.Services;

public sealed class SimulationApiClient
{
    private readonly HttpClient _http;

    public SimulationApiClient(HttpClient http) => _http = http;

    public async Task<int> StartAsync(CancellationToken ct = default)
    {
        using var response = await _http.PostAsync("api/Simulation", content: null, ct);
        response.EnsureSuccessStatusCode();

        var started = await response.Content.ReadFromJsonAsync<SimulationStartedResponse>(ct)
            ?? throw new InvalidOperationException("Empty simulation start response.");

        return started.Id;
    }

    public async Task SimulateMatchdayAsync(int runId, int matchday, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/Simulation/{runId}/matchday/{matchday}", content: null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<StandingRow>> GetStandingsAsync(int runId, CancellationToken ct = default)
    {
        var rows = await _http.GetFromJsonAsync<List<StandingRow>>($"api/Simulation/{runId}/standings", ct);
        return rows ?? [];
    }
}

public sealed record SimulationStartedResponse(int Id);

public sealed record StandingRow(string Group, string Team, int Rank, int Points, string FlagPath);
